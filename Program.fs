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

    let secureResourceHandler : HttpHandler =
        fun ctx ->
            let handleAuth : HttpHandler =
                Response.ofPlainText "Hello authenticated user"

            let ttt = 
                match ctx.Request.Headers.TryGetValue("Authorization") with
                | true, value -> value
                | _ -> Microsoft.Extensions.Primitives.StringValues.Empty
            printfn $"Secure resource handler: {ttt}"

            ifAuthenticated authScheme handleAuth ctx

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
            secureResourceEndpoint
            greeterHandler
            fortuneHandler
            testAuth
            // loginHandler
            // post "/upload" uploadHandler
        ]

    [<EntryPoint>]
    let main args =
        let bldr = WebApplication.CreateBuilder(args)

        bldr.Services
            .AddAuthorization()
            .AddAntiforgery()
            .AddFalcoOpenApi()
            .AddSwaggerGen()
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer( fun options ->
                options.TokenValidationParameters <- TokenValidationParameters(
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "https://localhost:5169", // Replace with your actual issuer
                    ValidAudience = "youraudience",        // Replace with your actual audience
                    IssuerSigningKey = SymmetricSecurityKey(
                        "edfe8827408477d5290538b0ca2177708d51bb6dee6e047019065f5474cdf942f0d3d882c07da3d344cd2f2a7cd0cb212ae8ee44a3c52a30043c20e66f706bea54071ee5aed7807a724af744048b9270c7a0b6b3f1fc0a089cfa8cd63249c1d2a069f22ff7c45c3b82a1ba8099774f311d147cbeb19e25f3d62ef73bd8f4f8230f16d92dbe0b4f514c8a74b7e83a507d29d37683afb9b155e7ac837337b879ee45194810104a5c9091a23efb538fc0bfef3414ba496cc022f876946641652093cc220cfc249c721b4ef2eb50e7b755ef2e1843e91f1cf516b157df5190495e15bd201db688e6076c53f791f85cc77d4ce5a3047ba15228d9da1c4007e6e767ee98b4fa75bb8858e4a53b2b0ef4b2f4e8dde50806424254458f631e00a112f43f" 
                        |> Convert.FromBase64String
                    ) // Replace with your secret key
                )
                options.Events <- JwtBearerEvents(
                    OnAuthenticationFailed = fun context ->
                        task {
                            printfn "Authentication failed: %A" context.Exception
                            return ()
                        }
                    // OnTokenValidated = fun context ->
                    //     task {
                    //         // Extract  claims from the token
                    //         let claimsPrincipal = context.Principal
                    //         let claims = claimsPrincipal.Claims |> Seq.toList

                    //         printfn "Claims: %A" claims

                    //         // Example: Validate a custom claim
                    //         let adminClaim =
                    //             claims
                    //             |> List.tryFind (fun claim -> claim.Type = "admin")

                    //         match adminClaim with
                    //         | Some claim when claim.Value = "true" ->
                    //             // Custom validation passed
                    //             printfn "Custom validation succeeded."
                    //             return ()
                    //         | _ ->
                    //             // Custom validation failed
                    //             printfn "Custom validation failed."
                    //             context.Fail("Invalid token: Missing or invalid 'admin' claim.")
                    //             return ()
                    //     }
                )
                // options.Authority <- "https://localhost:5169" // Optional if you are using a trusted authority
                // options.MapInboundClaims <- false
            )
        |> ignore

        let wapp = bldr.Build()

        wapp.UseHttpsRedirection()
            .UseAntiforgery()
            .UseSwagger()
            .UseSwaggerUI()
            .UseAuthentication()
            .UseAuthorization()
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
