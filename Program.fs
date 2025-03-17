namespace OpenApi

open System
open Falco
open Falco.OpenApi
open Falco.Routing
open Falco.Markup
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.IdentityModel.Tokens
open System.Text
open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open OpenApi.WelcomePage

type FortuneInput =
    { Name : string }

type Fortune =
    { Description : string }
    static member Create age input =
        match age with
        | Some age when age > 0 ->
            { Description = $"{input.Name}, you will experience great success when you are {age + 3}." }
        | _ ->
            { Description = $"{input.Name}, your future is unclear." }

type Greeting =
    { Message : string }

type LoginInput =
    { Username : string
      Password : string }

type LoginResponse =
    { Token : string }



module Program =
    open System.IO
    open Falco.Markup

    // let html = renderHtml welcomePage
    let authenticate
        (authScheme : string)
        (next : bool -> HttpHandler) : HttpHandler = fun ctx ->
        task {
            // this is stubbed out to throw null!!!!!!
            let authResult = AuthenticationHttpContextExtensions.AuthenticateAsync(ctx) |> Async.AwaitTask |> Async.RunSynchronously
            // let! authenticateResult = ctx.AuthenticateAsync()
            printfn "Authenticate result: %A" authResult.Succeeded
            return! next authResult.Succeeded ctx
        }

    /// Authenticate the current request using the default authentication scheme.
    ///
    /// Proceeds if the authentication status of current `IPrincipal` is true.
    ///
    /// The default authentication scheme can be configured using
    /// `Microsoft.AspNetCore.Authentication.AuthenticationOptions.DefaultAuthenticateScheme.`
    let ifAuthenticated
        (authScheme : string)
        (handleOk : HttpHandler) : HttpHandler =
        authenticate authScheme (fun authenticateResult ctx ->
            if authenticateResult then
                handleOk ctx
            else
                ctx.ForbidAsync())

    let authScheme = JwtBearerDefaults.AuthenticationScheme

    printfn "Auth Sheme: %A" authScheme

    let apiKeyHeaderName = "X-API-Key"
    let validApiKey = "dXNlcjpwYXNzd2Q=" // user:passwd

    let authenticateWithBasicAuth (next: HttpHandler) : HttpHandler = fun ctx ->
        task {
            let authHeader = ctx.Request.Headers.["Authorization"].ToString()
            if authHeader.StartsWith("Basic ") then
                let encodedCredentials = authHeader.Substring("Basic ".Length).Trim()
                let decodedCredentials = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials))
                let parts = decodedCredentials.Split(':')
                printfn "Decoded credentials: %A" decodedCredentials
                if parts.Length = 2 && parts.[0] = "user" && parts.[1] = "passwd" then
                // if not (String.IsNullOrWhiteSpace decodedCredentials) then
                    return! next ctx
                else
                    ctx.Response.StatusCode <- StatusCodes.Status401Unauthorized
                    return! ctx.Response.WriteAsync("Unauthorized: Invalid credentials")
            else
                ctx.Response.StatusCode <- StatusCodes.Status401Unauthorized
                return! ctx.Response.WriteAsync("Unauthorized: Missing or invalid Authorization header")
        }

    let secureResourceHandler : HttpHandler =
        fun ctx ->
            let handleAuth : HttpHandler =
                Response.ofPlainText "Hello authenticated user"

            authenticateWithBasicAuth handleAuth ctx

    let secureResourceEndpoint =
        get "/secure" secureResourceHandler
        |> OpenApi.name "SecureResource"
        |> OpenApi.summary "A secure resource"
        |> OpenApi.description "This endpoint is a secure resource that requires authentication."
        |> OpenApi.returnType typeof<string>


    let testAuth =
        mapGet "/test/{name?}"
            (fun req ->
                printfn $"MATCHED TO GET /test/{req?name.AsString()}"
                match req?name.AsString() with
                | name when not (String.IsNullOrWhiteSpace(name)) ->
                    printfn "Name: %s" name
                    // let isAuthorized = "Bearer yourtoken" = auth
                    // secureResourceHandler
                    $"Hello {name}!"
                | _ -> 
                    printfn "Unauthorized"
                    "Unauthorized"
            )
            Response.ofPlainText
                |> OpenApi.name "Test Auth"
                |> OpenApi.summary "Test authentication"
                |> OpenApi.description "This endpoint will test authentication."
                |> OpenApi.route [
                    { Type = typeof<string>; Name = "Name"; Required = false } ]
                |> OpenApi.query [
                    { Type = typeof<int>; Name = "Age"; Required = false } ]
                |> OpenApi.acceptsType typeof<string>
                |> OpenApi.returnType typeof<string>

    let rootHandler =
        get "/" (Response.ofHtml WelcomePage.welcomePage)
        |> OpenApi.name "Welcome"
        |> OpenApi.summary "Welcome to the Healix API"
        |> OpenApi.description "This is the root endpoint of the Healix API. It provides a welcome message and instructions on how to request a token."
        |> OpenApi.returnType typeof<string>

    let greeterHandler =
        mapGet "/hello/{name?}"
                (fun route ->
                    let auth = route.TryGetString("auth")
                    let name = route?name.AsString("world")
                    let age = route?age.AsIntOption()

                    let message =
                        match age with
                        | Some a when a > 0 -> $"Hello {name}, you are {a} years old!"
                        | Some _ -> $"Hello {name}, you are ageless!"
                        | _ -> $"Hello {name}!"

                    { Message = message })
                Response.ofJson
                |> OpenApi.name "Greeting"
                |> OpenApi.summary "A friendly greeter"
                |> OpenApi.description "This endpoint will provide a customized greeting based on the name and age (optional) provided."
                |> OpenApi.route [
                    { Type = typeof<string>; Name = "Name"; Required = false } ]
                |> OpenApi.query [
                    { Type = typeof<int>; Name = "Age"; Required = false } ]
                |> OpenApi.acceptsType typeof<string>
                |> OpenApi.returnType typeof<Greeting>

    let fortuneHandler =
        mapPost "/fortune"
                (fun r -> r?age.AsIntOption())
                (fun ageOpt ->
                    Request.mapJson<FortuneInput> (Fortune.Create ageOpt >> Response.ofJson))
                |> OpenApi.name "Fortune"
                |> OpenApi.summary "A mystic fortune teller"
                |> OpenApi.description "Get a glimpse into your future, if you dare."
                |> OpenApi.query [
                    { Type = typeof<int>; Name = "Age"; Required = false } ]
                |> OpenApi.acceptsType typeof<FortuneInput>
                |> OpenApi.returnType typeof<Fortune>

    // let loginHandler =
    //     mapPost "/login"
    //         (fun req ->
    //             Request.mapJson<LoginInput> (fun loginInput ->
    //                 // Validate user credentials and return a JWT token (placeholder)
    //                 if loginInput.Username = "user" && loginInput.Password = "password" then
    //                     let claims = [ Claim(ClaimTypes.Name, loginInput.Username) ]
    //                     let key = SymmetricSecurityKey(Encoding.UTF8.GetBytes("yoursecret"))
    //                     let creds = SigningCredentials(key, SecurityAlgorithms.HmacSha256)
    //                     let token = JwtSecurityToken(
    //                         issuer = "yourissuer",
    //                         audience = "youraudience",
    //                         claims = claims,
    //                         expires = System.DateTime.Now.AddMinutes(30.0),
    //                         signingCredentials = creds
    //                     )
    //                     let tokenString = JwtSecurityTokenHandler().WriteToken(token)
    //                     { Token = tokenString } |> Response.ofJson
    //                 else
    //                     Response.withStatusCode 401 >> Response.ofPlainText "Invalid credentials"
    //             )
    //         )
    //         Response.ofJson
    //         |> OpenApi.name "Login"
    //         |> OpenApi.summary "Authenticate a user"
    //         |> OpenApi.description "Authenticate a user and receive a JWT token."
    //         |> OpenApi.acceptsType typeof<LoginInput>
    //         |> OpenApi.returnType typeof<LoginResponse>

    // let responseTemplate color content =
    //     Elem.div [ Attr.class' "heading" ] [
    //     Text.h1 "Hello world!" ]

    // let uploadHandler context: Task  =
        
    //     task {
    //         // Falco can also use aspnet's features directly
    //         // but offers an F# API for ease of use
    //         let! form = Request.getForm context
    //         // let! auth = Request.ifAuthenticated "sss" (fun _ -> task { return true }) context

    //         let extractedFile =
    //             // extract the file safely from the
    //             // IFormFileCollection in the http context
    //             form.Files
    //             |> Option.bind (fun form ->
    //                 // try to extract the uploaded file named after the "name" attribute in html
    //                 // GetFile returns null if no file is present, so we safely convert it into an optional value
    //                 form.GetFile "my-uploaded-file" |> Option.ofObj)

    //         match extractedFile with
    //         | Some file ->
    //             // if the file is present in the request, then we can do anything we want here
    //             // from validating size, extension, content type, etc., etc.

    //             // For our use case we'll create a disposable stream reader to get the text content of the file
    //             use reader = new StreamReader(file.OpenReadStream())
    //             // in our simple use case we'll just read the content into a single string
    //             let! content = reader.ReadToEndAsync()

    //             // we'll write the file to disk just as a sample
    //             // we could upload it to S3, Google Buckets, Azure Storage as well
    //             do! File.WriteAllTextAsync($"./{Guid.NewGuid()}.txt", content)

    //             // We received a file and we've "processed it" successfully
    //             let content = responseTemplate "green" content
    //             // send our HTML content to the client and that's it
    //             return! Response.ofHtml content context
    //         | None ->
    //             // The file was not found in the request return something
    //             let content = responseTemplate "tomato" "The file was not provided"

    //             return! context |> Response.withStatusCode 400 |> Response.ofHtml content
    //     }




    

    let endpoints =
        [
            rootHandler
            secureResourceEndpoint
            greeterHandler
            fortuneHandler
            testAuth
            // post "/upload" uploadHandler
        ]

    [<EntryPoint>]
    let main args =
        let bldr = WebApplication.CreateBuilder(args)

        bldr.Services
            .AddAuthorization()
            .AddAntiforgery()
            .AddFalcoOpenApi()
            .AddSwaggerGen(
                fun opts ->
                    opts.SwaggerDoc("v1", Microsoft.OpenApi.Models.OpenApiInfo(Title = "Falco API", Version = "v1"))
                    opts.AddSecurityDefinition("Basic", Microsoft.OpenApi.Models.OpenApiSecurityScheme(
                        Description = "Basic Authentication with username and password.",
                        Name = "Authorization",
                        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                        Scheme = "Basic"
                    ))
                    let securityRequirement = Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
                    securityRequirement.Add(
                        Microsoft.OpenApi.Models.OpenApiSecurityScheme(
                            Reference = Microsoft.OpenApi.Models.OpenApiReference(
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Basic"
                            )
                        ), [||]
                    )
                    opts.AddSecurityRequirement(securityRequirement)
            )
        |> ignore

        let wapp = bldr.Build()

        wapp.UseHttpsRedirection()
            .UseAntiforgery()
            .UseSwagger()
            .UseSwaggerUI()
            .UseRouting()
            .UseFalco(endpoints)
        |> ignore

        wapp.Run()

        0

// curl -X 'POST' \
//   'http://localhost:5169/secure' \
//   -H 'accept: application/json' \
//   -H 'Content-Type: text/plain' \
//   -H 'Authorization: Bearer yourtoken' \
//   -d 'string'
