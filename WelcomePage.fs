namespace OpenApi

open Falco.Markup

module WelcomePage =
    let welcomePage =
        Elem.html [ Attr.lang "en" ] [
            Elem.head [] [
                Elem.title [] [ Text.raw "Welcome to Healix API" ]
                Elem.link [ Attr.rel "stylesheet"; Attr.href "https://cdn.jsdelivr.net/npm/bulma@1.0.0/css/bulma.min.css" ]
                Elem.style [] [
                    Text.raw """
                    :root {
                        --primary-color: #05435c;
                        --secondary-color: #EA7E7D;
                        --background-light: #FFFFFF;
                        --background-dark: #121212;
                        --text-light: #434D56;
                        --text-dark: #E6E6E6;
                    }
                    body.light-mode {
                        background-color: var(--background-light);
                        color: var(--text-light);
                    }
                    body.dark-mode {
                        background-color: var(--background-dark);
                        color: var(--text-dark);
                    }
                    // .welcome-backdrop {
                    //     background-color: var(--primary-color);
                    //     height: 100vh;
                    //     display: flex;
                    //     justify-content: center;
                    //     align-items: center;
                    //     padding: 1rem; /* Add padding for mobile responsiveness */
                    // }
                    .welcome-backdrop {
                        background-color: var(--primary-color);
                        height: 100vh;
                        display: flex;
                        justify-content: center;
                        align-items: center;
                        background-image: url(https://dev.healix.antidote-ai.com/antidote_logo_slice.svg);
                        background-repeat: no-repeat;
                        background-position: center;
                        background-size: cover;
                        padding: 1rem; /* Add padding for mobile responsiveness */
                    }
                    .box {
                        max-width: 90%; /* Ensure box fits on smaller screens */
                        margin: 0 auto; /* Center the box */
                        background: var(--background-light);
                        box-shadow: 0px 4px 6px rgba(0, 0, 0, 0.1);
                        border-radius: 8px;
                    }
                    body.dark-mode .box {
                        background: var(--background-dark);
                    }
                    .title {
                        color: var(--secondary-color);
                    }
                    .subtitle {
                        color: var(--text-light);
                    }
                    body.dark-mode .subtitle {
                        color: var(--text-dark);
                    }
                    a {
                        color: #E46B4C;
                    }
                    a:hover {
                        text-decoration: underline;
                    }
                    .button.is-link {
                        background-color: #0E475F;
                        color: #FFFFFF;
                    }
                    .button.is-link:hover {
                        background-color: #DF5F2E;
                    }
                    """
                ]
                Elem.script [] [
                    Text.raw """
                    function toggleMode() {
                        const body = document.body;
                        body.classList.toggle('dark-mode');
                        body.classList.toggle('light-mode');
                    }
                    """
                ]
            ]
            Elem.body [ Attr.class' "light-mode" ] [
                Elem.div [ Attr.class' "welcome-backdrop" ] [
                    Elem.div [ Attr.class' "container has-text-centered" ] [
                        Elem.div [ Attr.class' "box" ] [
                            Elem.h1 [ Attr.class' "title is-3" ] [ Text.raw "Welcome to Healix API" ] // Adjusted title size for better scaling
                            Elem.p [ Attr.class' "subtitle is-5" ] [ // Adjusted subtitle size
                                Text.raw "Explore our API capabilities and documentation for seamless integration. We follow the "
                                Text.strong "OpenAPI v3 standard"
                                Text.raw "."
                            ]
                            Elem.a [ Attr.href "/swagger"; Attr.class' "button is-link is-medium is-rounded" ] [
                                Text.raw "View Swagger Documentation"
                            ]
                            Elem.div [ Attr.class' "content mt-5" ] [
                                Elem.h2 [ Attr.class' "title is-4" ] [ Text.raw "What is OpenAPI?" ]
                                Elem.p [] [
                                    Text.raw "The OpenAPI Specification (OAS) is a widely adopted standard for describing RESTful APIs. It provides a format for defining endpoints, request/response formats, and more."
                                ]
                                Elem.p [] [
                                    Text.raw "To learn more, check out these resources:"
                                ]
                                Elem.div [ Attr.class' "columns is-multiline" ] [
                                    Elem.div [ Attr.class' "column is-full-mobile is-half-tablet" ] [ // Adjusted for mobile responsiveness
                                        Elem.div [ Attr.class' "card" ] [
                                            Elem.div [ Attr.class' "card-content" ] [
                                                Elem.p [ Attr.class' "title is-5" ] [ Text.raw "Official OpenAPI Specification" ]
                                                Elem.p [ Attr.class' "subtitle is-6" ] [
                                                    Text.raw "Learn about the official OpenAPI Specification and its features."
                                                ]
                                            ]
                                            Elem.footer [ Attr.class' "card-footer" ] [
                                                Elem.a [ Attr.href "https://swagger.io/specification/"; Attr.target "_blank"; Attr.class' "card-footer-item" ] [
                                                    Text.raw "Visit"
                                                ]
                                            ]
                                        ]
                                    ]
                                    Elem.div [ Attr.class' "column is-full-mobile is-half-tablet" ] [ // Adjusted for mobile responsiveness
                                        Elem.div [ Attr.class' "card" ] [
                                            Elem.div [ Attr.class' "card-content" ] [
                                                Elem.p [ Attr.class' "title is-5" ] [ Text.raw "OpenAPI GitHub Repository" ]
                                                Elem.p [ Attr.class' "subtitle is-6" ] [
                                                    Text.raw "Explore the OpenAPI GitHub repository for more details and contributions."
                                                ]
                                            ]
                                            Elem.footer [ Attr.class' "card-footer" ] [
                                                Elem.a [ Attr.href "https://github.com/OAI/OpenAPI-Specification"; Attr.target "_blank"; Attr.class' "card-footer-item" ] [
                                                    Text.raw "Visit"
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                                Elem.p [ Attr.class' "mt-4" ] [
                                    Text.raw "To request a dev token, please contact us at "
                                    Elem.a [
                                        Attr.href "mailto:support@antidote-ai.com?subject=API%20Key%20Request&body=Dear%20Antidote%20AI%20Support%2C%0D%0A%0D%0AI%20would%20like%20to%20request%20an%20API%20key.%20Below%20are%20my%20details%3A%0D%0A%0D%0AName%3A%20%0D%0AEmail%3A%20%0D%0ACompany%3A%20%0D%0ATelephone%3A%20%0D%0AAdditional%20Details%3A%20%0D%0A%0D%0AThank%20you%2C%0D%0A%5BYour%20Name%5D"
                                    ] [ Text.raw "support@antidote-ai.com" ]
                                    Text.raw ". Please include the following information: your name, email, company, telephone, and any additional details about your request."
                                ]
                            ]
                            Elem.button [ Attr.class' "button is-small is-dark mt-4"; Attr.onclick "toggleMode()" ] [
                                Text.raw "Toggle Dark/Light Mode"
                            ]
                        ]
                    ]
                ]
            ]
        ]
