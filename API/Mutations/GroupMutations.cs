using System.Security.Claims;
using DAL.Models.Groups;
using DAL.Models.Invitations;
using DAL.Models.UserGroups;
using DAL.Repositories;
using HotChocolate.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace API.Mutations;

[ExtendObjectType("Mutation")]
public class GroupMutations
{
    [Authorize]
    public async Task<Group?> CreateGroup([FromServices] IGroupRepository groupRepository,
        [FromServices] IUserGroupRepository userGroupRepository,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromServices] IPayDueToRepository payDueToRepository,
        GroupInsertInput groupInsertInput)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return null;
        var currentGroup = await groupRepository.InsertAsync(groupInsertInput.ToGroupInsertDto(Guid.Parse(userId)));
        if (currentGroup == null) throw new Exception("Group not created");
        var userGroup = new UserGroupInsertDto
        {
            UserId = currentGroup.OwnerId,
            GroupId = currentGroup.Id
        };
        await userGroupRepository.InsertAsync(userGroup);
        await payDueToRepository.InitPayDueToAsync(currentGroup.Id, currentGroup.OwnerId);
        return groupRepository.GetByIdAsync(currentGroup.Id).Result;
    }

    [Authorize]
    public async Task<Invitation?> InviteUser([FromServices] IInvitationRepository invitationRepository,
        [FromServices] IGroupRepository groupRepository,
        [FromServices] IUserRepository userRepository,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        InvitationInsertDto invitationInsertDto)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return null;
        var group = await groupRepository.GetByIdAsync(invitationInsertDto.GroupId);
        if (group == null) throw new Exception("Group not found");
        var user = userRepository.GetById(invitationInsertDto.UserId);
        if (await user.FirstAsync() == null) throw new Exception("User not found");
        if (Guid.Parse(userId) == invitationInsertDto.UserId) throw new Exception("You can't invite yourself ???");

        return await invitationRepository.InsertAsync(invitationInsertDto);
    }

    [Authorize]
    public async Task<string> UploadGroupImagePicture(Guid groupId,
        [FromServices] IUserRepository userRepository,
        [FromServices] IGroupRepository groupRepository,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromServices] IDistributedCache distributedCache)
    {
        var userIdString = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                           throw new Exception("User not found");
        var userId = Guid.Parse(userIdString);

        var group = await groupRepository.GetByIdAsync(groupId) ?? throw new Exception("Group not found");
        var user = await userRepository.GetByIdAsync(userId) ?? throw new Exception("User not found");

        if (group.OwnerId != user.Id) throw new Exception("You are not the owner of this group");

        var token = Guid.NewGuid().ToString();
        await distributedCache.SetStringAsync(token, groupId.ToString(), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });
        var baseUrl = $"http://localhost:{DockerEnv.ApiPort}";

        return $"{baseUrl}/groupimages/{token}";
    }

    [Authorize]
    public async Task<string> GetGroupPdfSumUp(Guid groupId,
        [FromServices] IUserRepository userRepository,
        [FromServices] IGroupRepository groupRepository,
        [FromServices] IUserGroupRepository userGroupRepository,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromServices] IDistributedCache distributedCache)
    {
        var userIdString = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                           throw new Exception("User not found");
        var userId = Guid.Parse(userIdString);

        var group = await groupRepository.GetByIdAsync(groupId) ?? throw new Exception("Group not found");
        var user = await userRepository.GetByIdAsync(userId) ?? throw new Exception("User not found");
        if (!await userGroupRepository.IsUserInGroup(userId, groupId)) throw new Exception("You are not in this group");

        var token = Guid.NewGuid().ToString();
        await distributedCache.SetStringAsync(token, groupId.ToString(), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });
        var baseUrl = $"http://localhost:{DockerEnv.ApiPort}";

        return $"{baseUrl}/groupsumups/{token}";
    }

    [Authorize]
    public async Task<Group?> ModifyGroup([FromServices] IGroupRepository groupRepository,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        GroupModifyDto groupModifyDto)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return null;
        var group = await groupRepository.GetByIdAsync(groupModifyDto.GroupId);
        if (group == null) throw new Exception("Group not found");
        return await groupRepository.ModifyAsync(Guid.Parse(userId), groupModifyDto);
    }

    [Authorize]
    public async Task<string> PayDuesToGroup(Guid groupId,
        [FromServices] IUserRepository userRepository,
        [FromServices] IGroupRepository groupRepository,
        [FromServices] IUserGroupRepository userGroupRepository,
        [FromServices] IUserExpenseRepository userExpenseRepository,
        [FromServices] IPayPalRepository payPalRepository,
        [FromServices] IPayDueToRepository payDueToRepository,
        [FromServices] IHttpContextAccessor httpContextAccessor)
    {
        // Basic validations
        var payerIdString = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                            throw new Exception("User not found");
        var payerId = Guid.Parse(payerIdString);

        var payer = await userRepository.GetByIdAsync(payerId) ?? throw new Exception("User not found");
        var group = groupRepository.GetById(groupId).First() ?? throw new Exception("Group not found");

        var isPayerInGroup = await userGroupRepository.IsUserInGroup(payerId, groupId);
        if (!isPayerInGroup) throw new Exception("You are not a member of the specified group");

        // At this point the payer is in the group and the group exists, hence we can proceed with the retrieval of the amount to be paid
        var payDueTo = await payDueToRepository.GetPayDueToAsync(groupId, payerId);
        var amountToPay = payDueTo.AmountToPay;
        var payeeId = payDueTo.PayToUserId;
        if (payeeId == null) throw new Exception("You don't have to pay anything");
        var payee = await userRepository.GetByIdAsync(payeeId.Value) ?? throw new Exception("Payee not found");

        // At this point we have the payer, the payee, the amount to pay and the group, hence we can proceed with the payment
        var payment = payPalRepository.CreatePaymentBetweenUsers(payer, payee, amountToPay);
        if (payment == null) throw new Exception("Payment failed");

        var approvalUrl = payment.links.FirstOrDefault(l => l.rel == "approval_url")?.href;

        if (string.IsNullOrEmpty(approvalUrl)) throw new Exception("Approval URL not found");

        // At this point the payment was successful, hence we can proceed with the update of the payDueTo
        if (payDueTo.PayToUserId != null)
        {
            await userGroupRepository.AddToBalanceAsync(payDueTo.PayToUserId.Value, groupId, -payDueTo.AmountToPay);
        }

        await userExpenseRepository.SetPaidAtAsync(groupId, payerId);

        payDueTo.AmountToPay = decimal.Zero;
        payDueTo.PayToUserId = null;
        await payDueToRepository.UpdateAsync(payDueTo);
        await userGroupRepository.ResetBalanceAsync(payerId, groupId);

        return approvalUrl;
    }

    [Authorize]
    public async Task<decimal> ManuallyValidatePayment(
        Guid payerId,
        Guid groupId,
        [FromServices] IUserRepository userRepository,
        [FromServices] IGroupRepository groupRepository,
        [FromServices] IUserGroupRepository userGroupRepository,
        [FromServices] IPayDueToRepository payDueToRepository,
        [FromServices] IUserExpenseRepository userExpenseRepository,
        [FromServices] IHttpContextAccessor httpContextAccessor
    )
    {
        var currentUserStringId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                                  throw new Exception("User not found");
        var currentUserId = Guid.Parse(currentUserStringId);
        var currentUser = await userRepository.GetByIdAsync(currentUserId);
        
        var payer = await userRepository.GetByIdAsync(payerId) ?? throw new Exception("Payer not found");
        var group = await groupRepository.GetByIdAsync(groupId) ?? throw new Exception("Group not found");
        var areUsersInGroup = await userGroupRepository.AreUsersInGroup(groupId, new List<Guid> {payerId, currentUserId});
        if (!areUsersInGroup) throw new Exception("Payer or logged in user are not in the group");
        
        var payDueTo = await payDueToRepository.GetPayDueToAsync(groupId, payerId);
        if (payDueTo.AmountToPay == 0) throw new Exception("Payer doesn't have to pay anything");
        if (payDueTo.PayToUserId == null) throw new Exception("Payer doesn't have to pay anyone");

        var amountPaid = payDueTo.AmountToPay;
        await userGroupRepository.ResetBalanceAsync(payerId, groupId);
        await userGroupRepository.AddToBalanceAsync(currentUserId, groupId, -amountPaid);

        payDueTo.AmountToPay = 0;
        payDueTo.PayToUserId = null;
        await payDueToRepository.UpdateAsync(payDueTo);
        await userExpenseRepository.SetPaidAtAsync(groupId, payerId);
        return amountPaid;
    }
}