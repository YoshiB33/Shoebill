using System.Net;
using Moq;
using Moq.Protected;
using Shoebill.Models;
using Shoebill.Services;

namespace Shoebill.Tests.Services;

public class TestApiService
{
    [Fact]
    public async Task GetServersAsync_ApiKey_Is_Null()
    {
        var service = new ApiService(new HttpClient());
        service.SetApiKey(null);
        
        await Assert.ThrowsAsync<ArgumentNullException>(service.GetServersAsync);
    }

    [Fact]
    public void GetServersAsync_Returns_Success()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\n  \"object\": \"list\",\n  \"data\": [\n    {\n      \"object\": \"server\",\n      \"attributes\": {\n        \"server_owner\": true,\n        \"identifier\": \"2bc172e4\",\n        \"internal_id\": 1,\n        \"uuid\": \"2bc172e4-6a09-4dc2-b832-2c859593290f\",\n        \"name\": \"Java 8\",\n        \"node\": \"Wings\",\n        \"sftp_details\": {\n          \"ip\": \"127.0.0.1\",\n          \"port\": 2022\n        },\n        \"description\": \"\",\n        \"limits\": {\n          \"memory\": 1024,\n          \"swap\": 0,\n          \"disk\": 1024,\n          \"io\": 500,\n          \"cpu\": 0,\n          \"threads\": null,\n          \"oom_disabled\": true\n        },\n        \"invocation\": \"java -Xms128M -Xmx1024M -Dterminal.jline=false -Dterminal.ansi=true -jar server.jar\",\n        \"docker_image\": \"ghcr.io/pterodactyl/yolks:java_8\",\n        \"egg_features\": [\n          \"eula\",\n          \"java_version\",\n          \"pid_limit\"\n        ],\n        \"feature_limits\": {\n          \"databases\": 0,\n          \"allocations\": 0,\n          \"backups\": 0\n        },\n        \"status\": null,\n        \"is_suspended\": false,\n        \"is_installing\": false,\n        \"is_transferring\": false,\n        \"relationships\": {\n          \"allocations\": {\n            \"object\": \"list\",\n            \"data\": [\n              {\n                \"object\": \"allocation\",\n                \"attributes\": {\n                  \"id\": 41,\n                  \"ip\": \"0.0.0.0\",\n                  \"ip_alias\": null,\n                  \"port\": 1234,\n                  \"notes\": null,\n                  \"is_default\": true\n                }\n              }\n            ]\n          },\n          \"variables\": {\n            \"object\": \"list\",\n            \"data\": [\n              {\n                \"object\": \"egg_variable\",\n                \"attributes\": {\n                  \"name\": \"Minecraft Version\",\n                  \"description\": \"The version of minecraft to download. \\r\\n\\r\\nLeave at latest to always get the latest version. Invalid versions will default to latest.\",\n                  \"env_variable\": \"MINECRAFT_VERSION\",\n                  \"default_value\": \"latest\",\n                  \"server_value\": \"1.8.8\",\n                  \"is_editable\": true,\n                  \"rules\": \"nullable|string|max:20\"\n                }\n              },\n              {\n                \"object\": \"egg_variable\",\n                \"attributes\": {\n                  \"name\": \"Server Jar File\",\n                  \"description\": \"The name of the server jarfile to run the server with.\",\n                  \"env_variable\": \"SERVER_JARFILE\",\n                  \"default_value\": \"server.jar\",\n                  \"server_value\": \"server.jar\",\n                  \"is_editable\": true,\n                  \"rules\": \"required|regex:/^([\\\\w\\\\d._-]+)(\\\\.jar)$/\"\n                }\n              },\n              {\n                \"object\": \"egg_variable\",\n                \"attributes\": {\n                  \"name\": \"Build Number\",\n                  \"description\": \"The build number for the paper release.\\r\\n\\r\\nLeave at latest to always get the latest version. Invalid versions will default to latest.\",\n                  \"env_variable\": \"BUILD_NUMBER\",\n                  \"default_value\": \"latest\",\n                  \"server_value\": \"latest\",\n                  \"is_editable\": true,\n                  \"rules\": \"required|string|max:20\"\n                }\n              }\n            ]\n          }\n        }\n      }\n    }\n  ],\n  \"meta\": {\n    \"pagination\": {\n      \"total\": 1,\n      \"count\": 1,\n      \"per_page\": 50,\n      \"current_page\": 1,\n      \"total_pages\": 1,\n      \"links\": {}\n    }\n  }\n}")
        };

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        var service = new ApiService(new HttpClient(handlerMock.Object));

        Assert.NotNull(service.GetServersAsync());
    }
    
    [Fact]
    public async Task GetServersAsync_Returns_Error()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.BadRequest,
            Content = new StringContent("{\n\n  \"errors\": [\n\n    {\n\n      \"code\": \"InvalidFilterQuery\",\n\n      \"status\": \"400\",\n\n      \"detail\": \"Requested filter(s) `0` are not allowed. Allowed filter(s) are `uuid, name, external_id, *`.\"\n\n    }\n\n  ]\n\n}")
        };

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        var service = new ApiService(new HttpClient(handlerMock.Object));
        service.SetApiKey(new ApiKey {Key = "Random key", Name = "Random name", ServerAdress = "127.0.0.1"});

        await Assert.ThrowsAsync<HttpRequestException>(service.GetServersAsync);
    }
}