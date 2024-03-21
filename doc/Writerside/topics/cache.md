# cache

The cache service uses a redis image to store data in ram. This service allows the back to generate secured tokens to be consumed by the client.
At the start of the project, this service was not intended to be used, but since the api uses a graphql engine, we found it more practical to serve files statically, using more traditional ways.
Therefore, the cache stores:
<tabs>
    <tab title="Justifications">
        A new token is generated when an authorized user wants to upload or download a justification.
        We want the justifications to be secured, since personal data can be included in it.
        Therefore, the following token is generated:
        <table>
            <tr>
                <th>Key</th>
                <th>Value</th>
            </tr>
            <tr>
                <th>Guid token</th>
                <th>Guid expenseId</th>
            </tr>
        </table>
    </tab>
    <tab title="Avatars">
        Avatars are less sensible files. That means we only generate a token when posting a new avatar:
        <table>
            <tr>
                <th>Key</th>
                <th>Value</th>
            </tr>
            <tr>
                <th>Guid token</th>
                <th>Guid userId</th>
            </tr>
        </table>
        To get the user's avatar, append <emphasis>userId.fileExtension</emphasis> to <emphasis>http://localhost:{API_PORT}/avatars/</emphasis>
    </tab>
</tabs>