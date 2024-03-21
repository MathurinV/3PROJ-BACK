# BACK-HOME

This is the documentation page for the BACK of 3PROJ project. It is composed of:
- a C# ASP.NET 8 **[api](api.md)** service with Entity Framework 8 and HotChocolate 14
- a PostgreSQL **[db](db.md)** service
- a Redis **[cache](cache.md)** service
- an alpine **[ftp](ftp.md)** service

## Docker configuration

The following docker-compose file is used:

<code-block lang="yaml">
networks:
  default:
  local:
services:
  api:
    image: api
    build:
      context: .
      dockerfile: API/Dockerfile
    networks:
      - default
      - local
    ports:
      - "${API_PORT}:8080"
    depends_on:
      - db
    healthcheck:
      test: [ "CMD", "curl", "-f", "http://localhost:8080/health" ]
      interval: 30s
      timeout: 10s
      retries: 5
  db:
    image: postgres:alpine
    volumes:
      - dbdata:/var/lib/postgresql/data
    networks:
      - local
    #  dev only:
    ports:
      - "${DB_PORT}:5432"
    healthcheck:
      test: [ "CMD", "pg_isready", "-U", "${POSTGRES_USER}" ]
      interval: 30s
      timeout: 10s
      retries: 5
  cache:
    image: redis:alpine
    networks:
      - local
    ports:
      - "${CACHE_PORT}:6379"
    restart: always
    healthcheck:
      test: [ "CMD", "redis-cli", "ping" ]
      interval: 30s
      timeout: 10s
      retries: 5
  ftp:
    image: delfer/alpine-ftp-server
    ports:
      - "21:21"
      - "21000-21010:21000-21010"
    volumes:
      - ./ftp_data:/home/vsftpd
    networks:
      - local
volumes:
  dbdata:
</code-block>

## Environment variables

A .env file is used to set the environment variables. You can change this file freely. Here is the default .env file provided:

<code-block>
# DATABASE
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_DB=moneyminder
# GOOGLE_AUTHENTICATION
GOOGLE_CLIENT_ID=test
GOOGLE_CLIENT_SECRET=test
# API
API_ENDPOINT=/graphql
# CLIENT
CLIENT_URL=http://localhost:8080
# CACHE
CACHE_PORT=6379
# FTP
PASV_ADDRESS=127.0.0.1
# FTP_JUSTIFICATIONS
FTP_JUSTIFICATIONS_USER=justificationsuser
FTP_JUSTIFICATIONS_PASS=P@ssw0rd
# AVATARS
FTP_AVATARS_USER=avatarsuser
FTP_AVATARS_PASS=P@ssw0rd
# DEV
DB_PORT=5432
API_PORT=3000
</code-block>

The following tabs explain the purpose of each environment variables

<tabs>
    <tab title="DATABASE">
        These environment variables are responsible for administration inside the <a href="db.md">db</a> service and to connect to the database from the <a href="api.md">api</a> using <a href="https://www.npgsql.org/doc/index.html">Npgsql</a>
        <table>
            <tr>
                <th>POSTGRES_USER</th>
                <th>POSTGRES_PASSWORD</th>
                <th>POSTGRES_DB</th>
            </tr>
            <tr>
                <td>Sets the username of the service</td>
                <td>Sets the password of the service</td>
                <td>Sets the database name of the service</td>
            </tr>
        </table>
    </tab>
    <tab title="GOOGLE_AUTHENTICATION">
        These environment variables are responsible for the management of the administrator's <a href="https://developers.google.com/api-client-library/dotnet/guide/aaa_oauth">OAuth 2.0 google credentials</a>
        <table>
            <tr>
                <th>GOOGLE_CLIENT_ID</th>
                <th>GOOGLE_CLIENT_SECRET</th>
            </tr>
            <tr>
                <td>Sets the id of the Google account managing the OAuth connection</td>
                <td>Sets the secret token used by the api</td>
            </tr>
        </table>
    </tab>
    <tab title="API">
        These environment variables are used to configure the api service
        <table>
            <tr>
                <th>API_ENDPOINT</th>
            </tr>
            <tr>
                <th>The endpoint from which the graphql api will be fetched</th>
            </tr>
        </table>
    </tab>
    <tab title="CLIENT">
        These environment variables are related to the client configuration.
        <table>
            <tr>
                <th>CLIENT_URL</th>
            </tr>
            <tr>
                <th>From the back perspective, this variable is used to configure CORS policies.</th>
            </tr>
        </table>
    </tab>
    <tab title="CACHE">
        These environment variables are used to configure the cache service.
        <table>
            <tr>
                <th>CACHE_PORT</th>
            </tr>
            <tr>
                <th>This environment variable is used to determine the port used by the cache service on the host machine </th>
            </tr>
        </table>
    </tab>
    <tab title="FTP">
        The following credentials are used to connect to the ftp service. there is a distinct user for each category of item uploaded on the server for security reasons.
        <tabs>
            <tab title = "FTP_JUSTIFICATIONS">
                <table>
                    <tr>
                        <th>FTP_JUSTIFICATIONS_USER</th>
                        <th>FTP_JUSTIFICATIONS_PASS</th>
                    </tr>
                    <tr>
                        <th>Defines the username for the justification upload user</th>
                        <th>Defines the password for the justification upload user</th>
                    </tr>
                </table>
            </tab>
            <tab title = "FTP_AVATARS">
                <table>
                    <tr>
                        <th>FTP_AVATARS_USER</th>
                        <th>FTP_AVATARS_USER</th>
                    </tr>
                    <tr>
                        <th>Defines the username for the avatar upload user</th>
                        <th>Defines the password for the avatar upload user</th>
                    </tr>
                </table>
            </tab>
        </tabs>
    </tab>
    <tab title="DEV">
        The following environment variables are used to define used ports on the host machine.
        <table>
            <tr>
                <th>DB_PORT</th>
                <th>API_PORT</th>
            </tr>
            <tr>
                <th>The postgres database port on the host machine</th>
                <th>The ASP.NET 8 api port on the host machine</th>
            </tr>
        </table>
    </tab>
</tabs>

## Installation

Execute the following command inside the Back solution root directory:
<code-block lang="bash">
docker compose up
</code-block>
This command will launch the 4 back services, and create a new folder called <emphasis>ftp_data</emphasis> inside the solution root folder.
<code-block>
ftp_data
├── avatars
└── justifications
</code-block>
Where avatars is the directory used to store users' avatars, and justifications is the directory where to store  expenses' justifications.

## Usage

The unique graphql endpoint is accessible with the following url:
<code>http://localhost:API_PORT/API_ENDPOINT</code> 

<emphasis>By default: <code>http://localhost:3000/graphql</code></emphasis>

This endpoint welcomes you with the banana cake pop graphql interface, which allows you to explore the architecture of the project.

<tip>Postgres is accessible at localhost:DB_PORT with:

<list>
<li>user=POSTGRES_USER</li>
<li>password=POSTGRES_PASSWORD</li>
<li>database=POSTGRES_DATABASE</li>
</list>
</tip>

<tip>Redis is accessible at localhost:CACHE_PORT</tip>

You can start populating the database by executing the solution's tests with the following command:

<code-block lang="bash">
dotnet test
</code-block>

<tip>To use the previous command, dotnet 8 will need to be installed on your computer.</tip>