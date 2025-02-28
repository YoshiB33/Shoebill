using System.Net;
using System.Text.Json;
using Moq;
using Moq.Protected;
using Shoebill.Models;
using Shoebill.Models.Api.Responses;
using Shoebill.Models.Api.Schemas;
using Xunit.Abstractions;

namespace Shoebill.Tests.Services;

public class ApiService
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ApiService(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    // The three following test are testing the GetServersAsync() function.
    [Fact]
    public async Task GetServersAsync_Returns_Error_If_ApiKey_Is_Null()
    {
        var service = new Shoebill.Services.ApiService(new HttpClient());
        service.SetApiKey(null);

        await Assert.ThrowsAsync<ArgumentNullException>(service.GetServersAsync);
    }

    [Fact]
    public async Task GetServersAsync_Returns_Success()
    {
        var handlerMock = new Mock<HttpMessageHandler>();

        // This is an example of a 200 ok response message. It has a compact formating to not take up too much space.
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(
                "{\"object\":\"list\",\"data\":[{\"object\":\"server\",\"attributes\":{\"server_owner\":true,\"identifier\":\"2bc172e4\",\"internal_id\":1,\"uuid\":\"2bc172e4-6a09-4dc2-b832-2c859593290f\",\"name\":\"Java 8\",\"node\":\"Wings\",\"sftp_details\":{\"ip\":\"127.0.0.1\",\"port\":2022},\"description\":\"\",\"limits\":{\"memory\":1024,\"swap\":0,\"disk\":1024,\"io\":500,\"cpu\":0,\"threads\":null,\"oom_disabled\":true},\"invocation\":\"java -Xms128M -Xmx1024M -Dterminal.jline=false -Dterminal.ansi=true -jar server.jar\",\"docker_image\":\"ghcr.io/pterodactyl/yolks:java_8\",\"egg_features\":[\"eula\",\"java_version\",\"pid_limit\"],\"feature_limits\":{\"databases\":0,\"allocations\":0,\"backups\":0},\"status\":null,\"is_suspended\":false,\"is_installing\":false,\"is_transferring\":false,\"relationships\":{\"allocations\":{\"object\":\"list\",\"data\":[{\"object\":\"allocation\",\"attributes\":{\"id\":41,\"ip\":\"0.0.0.0\",\"ip_alias\":null,\"port\":1234,\"notes\":null,\"is_default\":true}}]},\"variables\":{\"object\":\"list\",\"data\":[{\"object\":\"egg_variable\",\"attributes\":{\"name\":\"Minecraft Version\",\"description\":\"The version of minecraft to download. Leave at latest to always get the latest version. Invalid versions will default to latest.\",\"env_variable\":\"MINECRAFT_VERSION\",\"default_value\":\"latest\",\"server_value\":\"1.8.8\",\"is_editable\":true,\"rules\":\"nullable|string|max:20\"}},{\"object\":\"egg_variable\",\"attributes\":{\"name\":\"Server Jar File\",\"description\":\"The name of the server jarfile to run the server with.\",\"env_variable\":\"SERVER_JARFILE\",\"default_value\":\"server.jar\",\"server_value\":\"server.jar\",\"is_editable\":true,\"rules\":\"regex\"}},{\"object\":\"egg_variable\",\"attributes\":{\"name\":\"Build Number\",\"description\":\"The build number for the paper release. Leave at latest to always get the latest version. Invalid versions will default to latest.\",\"env_variable\":\"BUILD_NUMBER\",\"default_value\":\"latest\",\"server_value\":\"latest\",\"is_editable\":true,\"rules\":\"required|string|max:20\"}}]}}}}],\"meta\":{\"pagination\":{\"total\":1,\"count\":1,\"per_page\":50,\"current_page\":1,\"total_pages\":1,\"links\":{}}}}")
        };

        var expectedResult = new ListServer("list", [
            new ListServer_Data("server", new Server(
                true, "2bc172e4", 1, "2bc172e4-6a09-4dc2-b832-2c859593290f", "Java 8",
                "Wings", new SFPT_details("127.0.0.1", 2022), "", new Server_Limits(
                    1024, 0, 1024, 500, 0, null, true),
                "java -Xms128M -Xmx1024M -Dterminal.jline=false -Dterminal.ansi=true -jar server.jar",
                "ghcr.io/pterodactyl/yolks:java_8", ["eula", "java_version", "pid_limit"],
                new Server_Feature_limits(0, 0, 0), null, false, false, false,
                new Server_relationships(new Allocations("list", [
                        new Allocation_Data("allocation",
                            new Allocation(41, "0.0.0.0", null, 1234, null, true))
                    ]),
                    new Variables("list", [
                        new Variable_data("egg_variable", new Egg_variable(
                            "Minecraft Version",
                            "The version of minecraft to download. Leave at latest to always get the latest version. Invalid versions will default to latest.",
                            "MINECRAFT_VERSION", "latest", "1.8.8", true, "nullable|string|max:20")),
                        new Variable_data("egg_variable", new Egg_variable("Server Jar File",
                            "The name of the server jarfile to run the server with.",
                            "SERVER_JARFILE", "server.jar", "server.jar", true, "regex")),
                        new Variable_data("egg_variable", new Egg_variable("Build Number",
                            "The build number for the paper release. Leave at latest to always get the latest version. Invalid versions will default to latest.",
                            "BUILD_NUMBER", "latest", "latest", true, "required|string|max:20"))
                    ]), null, null)))
        ], new ListServer_Meta(new Pagination(1,1,50,1,1, new object())));

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        var service = new Shoebill.Services.ApiService(new HttpClient(handlerMock.Object));
        service.SetApiKey(new ApiKey { Key = "A random key", ServerAdress = "127.0.0.1" });
        service.CurrentServerUuid = "00000000-0000-0000-0000-000000000001";
        var actualResult = await service.GetServersAsync();
        Assert.Equivalent(expectedResult, actualResult);
    }

    [Fact]
    public async Task GetServersAsync_Returns_Error_If_Without_Permission()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.BadRequest,
            Content = new StringContent(
                "{\n  \"errors\": [\n    {\n      \"code\": \"AccessDeniedHttpException\",\n      \"status\": \"403\",\n      \"detail\": \"This action is unauthorized.\"\n    }\n  ]\n}")
        };

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        var service = new Shoebill.Services.ApiService(new HttpClient(handlerMock.Object));
        service.SetApiKey(new ApiKey
        {
            Key = "Random bad key", ServerAdress = "127.0.0.1" /*An example of an ApiKey that in this case is bad.*/
        });

        await Assert.ThrowsAsync<HttpRequestException>(service.GetServersAsync);
    }

    // The three following functions will test the GetServerAsync() function.
    [Fact]
    public async Task GetServerAsync_Returns_Error_If_ApiKey_Is_Null()
    {
        var service = new Shoebill.Services.ApiService(new HttpClient());
        service.SetApiKey(null);

        await Assert.ThrowsAsync<ArgumentNullException>(service.GetServersAsync);
    }

    [Fact]
    public async Task GetServerAsync_Returns_Success()
    {
        var handlerMock = new Mock<HttpMessageHandler>();

        // This is an example of a 200 ok response message. It has a compact formating to not take up too much space.
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(
                "{\"object\":\"server\",\"attributes\":{\"server_owner\":false,\"identifier\":\"4b43467c\",\"internal_id\":60,\"uuid\":\"4b43467c-6d78-411c-8018-3f2a75ccf6b3\",\"name\":\"Vanilla minecraft\",\"node\":\"Node 4\",\"sftp_details\":{\"ip\":\"local.wings\",\"port\":2022},\"description\":\"\",\"limits\":{\"memory\":4096,\"swap\":0,\"disk\":12700,\"io\":500,\"cpu\":0,\"threads\":null,\"oom_disabled\":true},\"invocation\":\"java -Xms128M -Xmx4096M -jar server.jar\",\"docker_image\":\"ghcr.io/pterodactyl/yolks:java_8\",\"egg_features\":[\"eula\",\"java_version\",\"pid_limit\"],\"feature_limits\":{\"databases\":0,\"allocations\":0,\"backups\":2},\"status\":null,\"is_suspended\":false,\"is_installing\":false,\"is_transferring\":false,\"relationships\":{\"allocations\":{\"object\":\"list\",\"data\":[{\"object\":\"allocation\",\"attributes\":{\"id\":23,\"ip\":\"127.0.0.1\",\"ip_alias\":null,\"port\":25668,\"notes\":null,\"is_default\":true}}]},\"variables\":{\"object\":\"list\",\"data\":[{\"object\":\"egg_variable\",\"attributes\":{\"name\":\"Server Jar File\",\"description\":\"The name of the server jarfile to run the server with.\",\"env_variable\":\"SERVER_JARFILE\",\"default_value\":\"server.jar\",\"server_value\":\"server.jar\",\"is_editable\":true,\"rules\":\"required|regex\"}},{\"object\":\"egg_variable\",\"attributes\":{\"name\":\"Server Version\",\"description\":\"The version of Minecraft Vanilla to install. Use \\\"latest\\\" to install the latest version, or use \\\"snapshot\\\" to install the latest snapshot. Go to Settings > Reinstall Server to apply.\",\"env_variable\":\"VANILLA_VERSION\",\"default_value\":\"latest\",\"server_value\":\"latest\",\"is_editable\":true,\"rules\":\"required|string|between:3,15\"}}]}}},\"meta\":{\"is_server_owner\":false,\"user_permissions\":[\"*\",\"admin.websocket.errors\",\"admin.websocket.install\",\"admin.websocket.transfer\"]}}")
        };

        var expectedResult = new Server(
            false, "4b43467c", 60, "4b43467c-6d78-411c-8018-3f2a75ccf6b3",
            "Vanilla minecraft", "Node 4", new SFPT_details("local.wings", 2022),
            "", new Server_Limits(4096, 0, 12700, 500, 0, null, true),
            "java -Xms128M -Xmx4096M -jar server.jar", "ghcr.io/pterodactyl/yolks:java_8",
            ["eula", "java_version", "pid_limit"], new Server_Feature_limits(
                0, 0, 2), null, false, false, false, new Server_relationships(
                new Allocations("list",
                    [new Allocation_Data("allocation", new Allocation(23, "127.0.0.1", null, 25668, null, true))]),
                new Variables("list", [
                    new Variable_data("egg_variable", new Egg_variable("Server Jar File",
                        "The name of the server jarfile to run the server with.",
                        "SERVER_JARFILE", "server.jar", "server.jar", true,
                        "required|regex")),
                    new Variable_data("egg_variable", new Egg_variable("Server Version",
                        "The version of Minecraft Vanilla to install. Use \"latest\" to install the latest version, or use \"snapshot\" to install the latest snapshot. Go to Settings > Reinstall Server to apply.",
                        "VANILLA_VERSION", "latest", "latest", true, "required|string|between:3,15"))
                ]), null, null));

        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        var service = new Shoebill.Services.ApiService(new HttpClient(handlerMock.Object));
        service.SetApiKey(new ApiKey { Key = "A random key", ServerAdress = "127.0.0.1" });
        service.CurrentServerUuid = "00000000-0000-0000-0000-000000000001";
        var actualResult = await service.GetServerAsync();

        Assert.Equivalent(expectedResult, actualResult);
    }

    [Fact]
    public async Task GetServerAsync_Returns_Error_If_Without_Permission()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.BadRequest,
            Content = new StringContent("""
                                        {
                                          "object": "server",
                                          "attributes": {
                                            "server_owner": false,
                                            "identifier": "4b43467c",
                                            "internal_id": 60,
                                            "uuid": "4b43467c-6d78-411c-8018-3f2a75ccf6b3",
                                            "name": "Vanilla minecraft",
                                            "node": "Node 4",
                                            "sftp_details": {
                                              "ip": "local.wings",
                                              "port": 2022
                                            },
                                            "description": "",
                                            "limits": {
                                              "memory": 4096,
                                              "swap": 0,
                                              "disk": 12700,
                                              "io": 500,
                                              "cpu": 0,
                                              "threads": null,
                                              "oom_disabled": true
                                            },
                                            "invocation": "java -Xms128M -Xmx4096M -jar server.jar",
                                            "docker_image": "ghcr.io/pterodactyl/yolks:java_8",
                                            "egg_features": [
                                              "eula",
                                              "java_version",
                                              "pid_limit"
                                            ],
                                            "feature_limits": {
                                              "databases": 0,
                                              "allocations": 0,
                                              "backups": 2
                                            },
                                            "status": null,
                                            "is_suspended": false,
                                            "is_installing": false,
                                            "is_transferring": false,
                                            "relationships": {
                                              "allocations": {
                                                "object": "list",
                                                "data": [
                                                  {
                                                    "object": "allocation",
                                                    "attributes": {
                                                      "id": 23,
                                                      "ip": "127.0.0.1",
                                                      "ip_alias": null,
                                                      "port": 25668,
                                                      "notes": null,
                                                      "is_default": true
                                                    }
                                                  }
                                                ]
                                              },
                                              "variables": {
                                                "object": "list",
                                                "data": [
                                                  {
                                                    "object": "egg_variable",
                                                    "attributes": {
                                                      "name": "Server Jar File",
                                                      "description": "The name of the server jarfile to run the server with.",
                                                      "env_variable": "SERVER_JARFILE",
                                                      "default_value": "server.jar",
                                                      "server_value": "server.jar",
                                                      "is_editable": true,
                                                      "rules": "required|regex:/^([\\w\\d._-]+)(\\.jar)$/"
                                                    }
                                                  },
                                                  {
                                                    "object": "egg_variable",
                                                    "attributes": {
                                                      "name": "Server Version",
                                                      "description": "The version of Minecraft Vanilla to install. Use \"latest\" to install the latest version, or use \"snapshot\" to install the latest snapshot. Go to Settings > Reinstall Server to apply.",
                                                      "env_variable": "VANILLA_VERSION",
                                                      "default_value": "latest",
                                                      "server_value": "latest",
                                                      "is_editable": true,
                                                      "rules": "required|string|between:3,15"
                                                    }
                                                  }
                                                ]
                                              }
                                            }
                                          },
                                          "meta": {
                                            "is_server_owner": false,
                                            "user_permissions": [
                                              "*",
                                              "admin.websocket.errors",
                                              "admin.websocket.install",
                                              "admin.websocket.transfer"
                                            ]
                                          }
                                        }
                                        """)
        };

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        var service = new Shoebill.Services.ApiService(new HttpClient(handlerMock.Object));
        service.SetApiKey(new ApiKey
        {
            Key = "Random bad key", Name = "Random name",
            ServerAdress = "127.0.0.1" /*An example of an ApiKey that in this case is bad.*/
        });
        service.CurrentServerUuid = "00000000-0000-0000-0000-000000000001";

        await Assert.ThrowsAsync<HttpRequestException>(service.GetServerAsync);
    }
}