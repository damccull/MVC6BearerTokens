# ASP.NET5 MVC6 Beta 6 Bearer Tokens

Apparently MVC6 does not have a native authorization server like previous versions. So I figured out how to make it work...mostly.

This project is an example hacked together from two different sources on how to do bearer tokens in MVC6:

* [Bit of Technology][bitoftech]
* [Matt Drekey's Stack Overflow answer][drekey]

It's built on Beta 6, so be aware that as new versions come out, this project may need updating, and I may not do that...feel free to send pull requests.

## Running the example

##### Step 1
Load the project in Visual Studio 2015 with MVC6 beta 6 and dnx beta 6 installed.

##### Step 2
Open either Powershell or the Package Manager Console (in Visual Studio) and change directory to the MVC6BearerTokens/src/RsaGen folder.

##### Step 3
Type `dnx . rsagen > key.txt`.

##### Step 4
Open key.txt in a text editor and copy the entire file.

##### Step 5
Open the project properties file and click the 'Debug' tab on the left. Create a new Environment Variable called 'rsa-key' and paste the json into its value. Save the properties file.

##### Step 6
Run the project.

##### Step 7
Using Fiddler or a similar tool, send a POST request to http://localhost:&lt;port&gt;/api/v1/account/register with the following:

```
POST http://localhost:<port>/api/v1/account/register HTTP/1.1
User-Agent: Fiddler
Host: localhost:<port>
Content-Length: 130
Content-Type: application/json; charset=utf-8

{
"userName": "testUser",
"email": "testuser@mailinator.com",
"password": "Password@123",
"confirmPassword": "Password@123"
}
```

You should receive a 201 Created response with a 'location' to '/token'.

##### Step 8
Issue another POST request to http://localhost:&lt;port&gt;/api/v1/account/token with the following:

```
POST http://localhost:<port>/api/v1/account/token HTTP/1.1
User-Agent: Fiddler
Host: localhost:<port>
Content-Length: 147
Content-Type: application/json; charset=utf-8

{
"email": "testuser@mailinator.com",
"password": "Password@123",
"rememberMe":"false",
"grantType": "password",
"clientId": "ngAngularApp"
}
```

You should get a response similar to:

```
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
Server: Microsoft-IIS/10.0
X-SourceFiles: =?UTF-8?B?YzpcdXNlcnNcZGF2aWRcUHJvamVjdHNcTVZDNkJlYXJlclRva2VuXHNyY1xNVkM2QmVhcmVyVG9rZW5cd3d3cm9vdFxhcGlcdjFcYWNjb3VudFx0b2tlbg==?=
X-Powered-By: ASP.NET
Date: Wed, 02 Sep 2015 02:06:21 GMT
Content-Length: 915

{"accessToken":"eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6bnVsbH0.eyJuYW1laWQiOiIxYzhjY2E3Mi05MjQxLTRiYjItOGUyZC0yYmY0N2RlYzc2Y2YiLCJ1bmlxdWVfbmFtZSI6InRlc3RVc2VyIiwiQXNwTmV0LklkZW50aXR5LlNlY3VyaXR5U3RhbXAiOiIxMTc2ZjU4NS05ZDIzLTQ4ZmQtYmM5NS02NjFjZThlMjY1NWYiLCJpc3MiOiJteWJlYXJlcnRva2VuYXBpIiwiYXVkIjoibXliZWFyZXJ0b2tlbmFwaSIsImV4cCI6MTQ0MTE2MzE4MSwibmJmIjoxNDQxMTU5NTgxfQ.Gr-hEZ5miwhaTd7p5-p645aiA0vfmVwrpex579WiEB5i3LWuYYzmgJvLkmJq8SLBFeTfTcQGvd-gKmkpqywDGLrTRlq81NhzHZESWFWs2kYB9TmOElyOhGXK5rQiflfK8KUNAzm82TjKUpTY22Y-ygqYjfovlVVTk1uL-UeOrMlCmzADF0hD40Bo2KB05QaJNZbxZEBAloHk_6aBFb_4EX1RUWIZwbeq-X5zkoSPWvVaAf4CwF1tVxuRo745E6cpUfJM-415meIhrwg9QaAbGO5fXO67ksXqiuytbKCu5GzaeD0BB6Je_3Zv4_dGWAgNmrUBjxZbE1riwjT-SrIPZQ","tokenType":"bearer","expiresIn":5,"clientId":"ngAngularApp","userName":"testUser","expires":"9/2/2015 2:11:21 AM","issued":"9/2/2015 2:06:21 AM","refreshToken":"1e2f5d5578db40c796c8eb857733a4c3"}
```

This response contains your refresh token, an access token,
and some other metadata that could be useful to your app.

##### Step 9
Copy the refreshToken from the previous response to your clipboard. You'll need it for this part.

Note: In this example the refresh tokens only stay valid for 5 minutes, but you can change that in the code. It's hard coded in the AccountController's Token action.

Issue another POST request to http://localhost:&lt;port&gt;/api/v1/account/token with the following:

```
POST http://localhost:<port>/api/v1/account/token HTTP/1.1
User-Agent: Fiddler
Host: localhost:<port>;
Content-Length: 115
Content-Type: application/json; charset=utf-8

{
"grantType": "refreshToken",
"clientId": "ngAngularApp",
"refreshToken": "3bcaf3ea8096429689d90c709dafb9ce"
}
```

Once again you should receive a 200 response with another set of token information, to include a new refresh token and a new access token.

Congratulations. Integrate it into your code and make it better and send me pull requests. Go now, and may the Force be with you.

[bitoftech]: http://bitoftech.net/2014/06/01/token-based-authentication-asp-net-web-api-2-owin-asp-net-identity/ "Bit of Technology: Token Based Authentication using ASP.NET Web API 2, Owin, and Identity"

[drekey]: http://stackoverflow.com/a/29698502/2371653 "Matt Drekey's answer"
