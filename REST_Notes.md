# REST API


https://restfulapi.net/

## Differences between REST and SOAP

Overall, REST is simpler to develop because it leverages the web, which is already in place, and the degree of freedom is limited (fewer choices to make, so simpler). SOAP offers several alternatives and is also slightly more difficult to develop, but offers more alternatives and areas to work.

### Protocol

- Soap is a protocol over HTTP
- REST does not mandate HTTP usage

### Data Format

- SOAP relies EXCLUSIVELY on XML to provide messaging services, which can become extremely complex in some cases, like accessing web services via JavaScript
- REST can use the data in comma separated values: CSV, JavaScript Object Notation: JSON or Really Simple Syndication (RSS)

### Statefulness

- RESTful Web services are completely stateless, management of the state of conversation is the complete responsibility of the client itself, there is no help to mitigate this on the server.
- SOAP Web services are stateless, but you can make one stateful by changing the code on the server

### Caching

- REST provides a good caching infrastructure over HTTP GET methods, enabling response data to be marked as cacheable or not-cachable. The ETag header is a good way to implement caching providing you have a 'cheap' way to compute what the value should be.


    - https://en.wikipedia.org/wiki/HTTP_ETag

- SOAP when using HTTP as a transfer method, is sent via HTTP POST requests. As HTTP POST is non-idempotent, it can not be cached at the HTTP lvel. So, SOAP responses should be cached using information given in the Response Cahcing Optimization Module.

### HTTP Verbs Used

Summary of HTTP Methods:

| HTTP Method | CRUD | Collection Resource (e.g. /users ) | Single Resource (e.g. /users/123 ) |
|-------------|------|------------------------------------|------------------------------------|
| POST | Create | 201 (Created), 'Location' header with link to /users/{id} containing new ID | Avoid using POST on a signle resource |
| GET | Read | 200 (OK), list of users. Use pagination, sorting, and filtering to navigate big lists | 200 (OK), single user. 404 (Not Found), if ID not found or invalid |
| PUT | Update/Replace | 405 (Method not allowed), unless you want to update every resource in the entire collection of resource | 200 (OK) or 204 (No Content). Use 404 (Not Found), if ID is not found or invalid |
| PATCH | Partial Update / Modify | 405 (Method not allowed), unless you want to delete the whole collection — use with caution | 200 (OK). 404 (Not Found), if ID not found or invalid |

- REST is primarily used over HTTP and uses the following for CRUD operations: GET, POST, PUT, DELETE & PATCH

    - GET:
        - Safe method : DOES NOT MODIFY THE DATA EVER
        - Retrieve resource representation/information only
        - Should be idempotent, making multiple identical requests must produce the same result every time until another API (POST or PUT) has changed the state of the resource on the server.
        - If the Request-URI refers to a data-producing process, it is produced data that shall be returned as the entity in the response and not the sorce text of the process, unless that text happens to be part of the process.

        - Response Codes:
            Resource Found: 200 (OK)
                - should include the response body, which is typically either XML or JSON content ()
            Resource NOT Found: 404 (Not Found)
            Malformed Request: 400 (Bad Request)

        - Example URIs:
            HTTP GET http://www.appdomain.com/users
            HTTP GET http://www.appdomain.com/users?size=20&page=5
            HTTP GET http://www.appdomain.com/users/123
            HTTP GET http://www.appdomain.com/users/123/address

    - POST
        - Create new resource into collection of resrouces, e.g. a file is subordinate to a directory containing it or a row is subordinate to a database table
        - Responses to this method are not cacheable, unless the response includes the appropriate Cache-Control or Expires headers
        - NOT safe nor is it idempotent, invoking two identical POST requests will result in two different resources contain the same information (minus resource id's)
        - Made on resource collections

        - Response Codes:
            Created: 201 (Created)
                - and should con tain an entity that describes the status of the request and referes to the new resource, and a Location header.
            Resource not identified: 200 (OK) || 204 (No Content) *

        - Example URIs:
            HTTP POST http://www.appdomain.com/users
            HTTP POST http://www.appdomain.com/users/123/accounts
        
        Cache-Control Header: https://en.wikipedia.org/wiki/Web_cache#Cache_control
        Expires Header: https://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html

        Location Header: https://en.wikipedia.org/wiki/HTTP_location

    - PUT
        - Update an existing resource (if it doesn't exist, server decision to create the new record)
        - If request passes through a cache and the Request-URI identifies one or more currenbtly cached entities, those entires SHOULD be treated as stale
        - Responses are not cacheable
        - Made on a single resource

        - Response Codes:
            Created: 201 (Created)
                - If the server creates a new resource, the origin server MUST inform the user agent via the HTTP
            Modified: 200 (Ok) || 204 (No Content)
                - If the server updates or creates a new record then SHOULD be sent to indicate success.
        
        - Example URIs:
            HTTP PUT http://www.appdomain.com/users/123
            HTTP PUT http://www.appdomain.com/users/123/accounts/456

    - For a further explanation of PUT vs. POST can be found here:
        https://restfulapi.net/rest-put-vs-post/

    - DELETE
        - Delete the specified resource (identified by Request-URI)
        - Idempotent, it is removed from the resource collection
        - Not cacheable
        - If the request pasess through a cache and the Request-URI identifies oneor more currently cached entities, those entries SHOULD be treated as stale.

        - Response Codes:
            Deleted: 200 (OK)
                - if the response includes an entity describing the status
            Deleted: 204 (No Content)
                - if the response does not include an entity
            Queued: 202 (Accepted)
                - if the action goes to a queue
            Not Found: 404 (Not Found)
                - subsequent calls with the same record, or if the referenced resource entity doesn't exist

        - Example URIs:
            HTTP DELETE http://www.appdomain.com/users/123
            HTTP DELETE http://www.appdomain.com/users/123/accounts/456

    - PATCH
        - Partial update on a resource: Applies a delta (diff) rather than replace the entire resource
        - PUT requests modify resource entities, PATCH should be used for partial updates, and PUT when replacing the entire record
        - Some challenges faced when using PATCH
            - PATCH is not supported universally in certain browsers, servers and web application frameworks; IE8, PHP, Tomcat, Django and many other software offerings have missing or broken support for it.

            Not as straight forward as PUT request:

                REQUEST: HTTP GET /users/1
                RESPONSE: { "id": 1, "username": "admin", "email": "email@example.org"}

            To modify the above record, the PATCH request would look like:

                REQUEST: HTTP PATCH /users/1
                REQUEST DATA: [{ "op": "replace", "path": "/email", "value": "new.email@example.org" }]

            Other examples of possible operations:
                [
                    { "op": "test",  "path": "/a/b/c",  "value": "foo"  },
                    { "op": "remove",  "path": "/a/b/c"  },
                    { "op": "add",  "path": "/a/b/c",  "value": [ "foo", "bar" ] },
                    { "op": "replace", "path": "/a/b/c",  "value": 42 },
                    { "op": "move",  "from": "/a/b/c",  "path": "/a/b/d" },
                    { "op": "copy", "from": "/a/b/d",  "path": "/a/b/e" }
                ]

https://restfulapi.net/http-methods/

- Soap also defines a binding to te HTTP protocol. When binding to HTTP, all SOAP requests are sent through HTTP POST.

### Security

- REST is based on HTTP - which itself is a very insecure protocol. It supports basic authentication and communication encryption through TLS. Any further security should be additionally implemented at the server.
- SOAP security is well standardized through WS-SECURITY, which is very feature-rich and easy to implement into application code.

### Asynchronous Processing

- Asynchronous request may be required when creating/updating a resource is time-consuming. In this case, REST suggest returning the HTTP response code 202, and sending the location of the queue where the status of task completion will be updaetd on a frequent interval. REST has pretty good support for async APIs in JAX-RS itself.
- If your application needs a guaranteed level of reliability and security, then SOAP 1.2 offers additional standards to ensure this type of operation. Things like WSRM - WS-Reliable-Messaging.











## Design and implement a REST API

### Identify the Resources - Object Modeling

- The first stop in designing a REST based API, is identifying the objects that wll be presented as resrouces.
- For a network-based application, object modeling is pretty much more straightforward. There can be many things such as devices, managed entities, routers, modems, etc.

- For this example, we will focus on two resources:
    Devices
    Configurations

- Here configuration may be a sub-resource of a device. A device can have many configuration options.
- Note that both objects/resources in our above model will have a unique identifier, which is the integer 'id' property.

### Create Model URIs

- Now when the object model is ready, it's time to decide the resource URIs.
- At this step, while designing the resource URIs - focus o the relationship between resources and their sub-resources.
- These resource URIs are endpoints for the API.
- URIs **do not use any verb or operation**. It's crucial not to include any verb in URIs, and **should all be nouns ONLY**.

- In this example application, a device is a top-level resource. And a configuration is a sub-resource under the device.

/devices
/devices/{id}

/configurations
/configurations/{id}

/devices/{id}/configurations
/devices/{id}/configurations/{configId}

### Determine Resource Representations

- Define the shape of the resource representation as JSON.

#### Collection Resource of Devices:

- When returning a collection resource, only include the most important information about that resource. This will keep the size of the response payload small, and so will the performance of the API.

```json
    {
        "devices": {
            "size": "2",
            "link": {
                "rel": "self",
                "href": "/devices"
            },
            "device": [
                {
                    "id": "12345",
                    "link": {
                        "rel": "self",
                        "href": "/devices/12345"
                    },
                    "deviceFamily": "apple-es",
                    "OSVersion": "10.3R2.11",
                    "platform": "SRX100B",
                    "serialNumber": "32423457",
                    "connectionStatus": "up",
                    "ipAddr": "192.168.21.9",
                    "name": "apple-srx_200",
                    "status": "active"
                },
                {
                    "id": "556677",
                    "link": {
                        "rel": "self",
                        "href": "/devices/556677"
                    },
                    "deviceFamily": "apple-es",
                    "OSVersion": "10.3R2.11",
                    "platform": "SRX100B",
                    "serialNumber": "6453534",
                    "connectionStatus": "up",
                    "ipAddr": "192.168.20.23",
                    "name": "apple-srx_200",
                    "status": "active"
                }
            ]
        }
    }
```

#### Signle Device Resource

- Opposite to collection URI, a single resource URI includes complete information about a particular device. It also includes a list o links to sub-resources and other supported operations. This will make your REST API HATEOAS driven.

```json
    {
        "device": {
            "id": "12345",
            "link": {
                "rel": "self",
                "href": "/devices/12345"
            },
            "deviceFamily": "apple-es",
            "OSVersion": "10.0R2.10",
            "platform": "SRX100-LM",
            "serialNumber": "32423457",
            "name": "apple-srx_100_lehar",
            "hostName": "apple-srx_100_lehar",
            "ipAddr": "192.168.21.9",
            "status": "active",
            "configurations": {
                "size": "2",
                "link": {
                    "rel": "self",
                    "href": "/configurations"
                },
                "configuration": [
                    {
                        "id": "42342",
                        "link": {
                            "rel": "self",
                            "href": "/configurations/42342"
                        }
                    },
                    {
                        "id": "675675",
                        "link": {
                            "rel": "self",
                            "href": "/configurations/675675"
                        }
                    }
                ]
            },
            "method": [
                {
                    "href": "/devices/12345/exec-rpc",
                    "rel": "rpc"
                },
                {
                    "href": "/devices/12345/synch-config",
                    "rel": "synch device configuration"
                }
            ]
        }
    }
```

https://restfulapi.net/hateoas/

#### Collection Resource of Configurations

- Similar to device collection representation, create configuration collection representation with only minimal information.
- Note that `configurations` collection representation inside device is similar to top-level configurations URI.
- The only difference is that configurations for a device are only two, so only two configuration items items are listed as subresources under the device.

```json
{
    "configurations": {
        "size": "20",
        "link": {
            "rel": "self",
            "href": "/configurations"
        },
        "configuration": [
            {
                "id": "42342",
                "link": {
                "rel": "self",
                "href": "/configurations/42342"
                }
            },
            {
                "id": "675675",
                "link": {
                "rel": "self",
                "href": "/configurations/675675"
                }
            }
        ]
    }
}
```

#### Single Configuration Resource

- Now, a single configuration resource representation must have all possible information about this resource – including relevant links

```json
{
    "configuration": {
        "id": "42342",
        "link": [
            {
                "rel": "self",
                "href": "/configurations/42342"
            },
            {
                "rel": "very big raw configuration script",
                "href": "/configurations/42342/raw"
            }
        ],
        "content": "<![CDATA[…]>",
        "status": "active"
    }
}
```

#### Collection Resource of Configuration under a Signle Device

- This sub-collection of configurations will be a subset of the primary collection of configurations and will be specific to a device only.
- As it is the subset of primary collection, **DO NOT create a different representation data field than primary collection. Use the same presentation as the primary collection.
- Notice the below is a sub-resource and has two links. One for its direct representation inside sub-collection i.e. `/devices/12345/configurations/333443` and others pointing to its location in the primary collection i.e. `/configurations/333443`
- Having two links is essential as you can provide access to a device-specific configuration more uniquely, and you will be able to mask some fields (if the design requires it), which shall not be visible in a secondary collection.

```json
{
    "configurations": {
        "size": "2",
        "link": {
            "rel": "self",
            "href": "/devices/12345/configurations"
        },
        "configuration": [
            {
                "id": "53324",
                "link": [
                    {
                        "rel": "self",
                        "href": "/devices/12345/configurations/53324"
                    },
                    {
                        "rel": "detail",
                        "href": "/configurations/53324"
                    }
                ]
            },
            {
                "id": "333443",
                "link": [
                    {
                        "rel": "self",
                        "href": "/devices/12345/configurations/333443"
                    },
                    {
                        "rel": "detail",
                        "href": "/configurations/333443"
                    }
                ]
            }
        ]
    }
}

```

#### Single Configuration Resource under a Single Device

- This representation should have either exactly a similar representation as of Configuration representation fromthe primary collection, OR you may mask a few fields.
- This subresource representation will also have an additional link to its primary presentation.

```json
{
    "configuration": {
        "id": "11223344",
        "link": [
            {
                "rel": "self",
                "href": "/devices/12345/configurations/11223344"
            },
            {
                "rel": "detail",
                "href": "/configurations/11223344"
            },
            {
                "rel": "raw configuration content",
                "href": "/configurations/11223344/raw"
            }
        ],
        "content": "<![CDATA[…]>",
        "status": "active"
    }
}
```

- Observations: 
    - Resource URIs are all nouns.
    - URIs are usually in two forms - collection of resources and singular resource
    - Collection may be in two forms primary collection and secondary collection. A secondary collection is a sub-collection from a primary collection only.
    - Each resource/collection contains at least one link i.e. to itself
    - Collections contain only the most important information about resources.
    - To get complete information about a resource, you only need to access its specific resource URI.
    - Represenations can have extra links (i.e. methods in a signle device). HEe method represents a POST method. You can also have more attributes or form links in an altogether new way.

#### Assigning HTTP Methods

- So our resource URIs and their representation are fixed now. Let's decide all the applications' possible operations and map those operations to the resource URIs.

e.g. A user of our network application can browse, create, update or delete devices from the network and create/deploy/remove the device configurations.

    ---

##### Browse all devices or configurations [Primary Collection]
    
HTTP GET /devices
HTTP GET /configurations

- OR if you have a large collection size, you can also apply paging and filtering, The below shows paging for the first 20 records.
HTTP GET /devices?startIndex=0&size=20
HTTP GET /configurations?startIndex=0&size=20

##### Browse all configurations under a device [Secondary Collecton]

HTTP GET /devices/{id}/configurations

- Mostly be a small-size collection, so there may not be a need to enable filtering or sorting here.

##### Browse a single configuration under a device

HTTP GET /devices/{id}/configurations/{configId}

- Subresource represenation will be either the same as or a subset of the primary presentation.

##### Create a device or configuration

HTTP POST /devices
HTTP POST /configurations

- Create is not an idempotant operation and in HTTP protocol - POST is also not idempotent. **Use POST**
- Please note that the request payload will not contain any `id` attribute, as the server is responsible for deciding it. The response to CREATE will look like the below:

```json
HTTP/1.1 201 Created
Content-Type: application/json
Location: http://example.com/network-app/configurations/678678

{
    "configuration": {
        "id": "678678",
        "link": [
            {
                "rel": "self",
                "href": "/configurations/678678"
            },
            {
                "rel": "raw configuration content",
                "href": "/configurations/678678/raw"
            }
        ],
        "content": "<![CDATA[…]>",
        "status": "active"
    }
}

```

##### Update a device or configuration

HTTP PUT /devices/{id}
HTTP PUT /configurations/{id}

- The update operation is an idempotent operation, and HTTP PUT is also idempotent, so use it for updates.

```json
HTTP/1.1 200 OK
Content-Type: application/json

{
    "configuration": {
        "id": "678678",
        "link": [
            {
                "rel": "self",
                "href": "/configurations/678678"
            },
            {
                "rel": "raw configuration content",
                "href": "/configurations/678678/raw"
            }
        ],
        "content": "<![CDATA[. updated content here .]]>",
        "status": "active"
    }
}

```

##### Remove a device or configuration

HTTP DELETE /devices/{id}
HTTP DELETE /configurations/{id}

- Removing is always a DELETE operation.
- A successful response SHOULD be 202 (Accepted) if the resource has been queued foir deletion (async operation), or 200 (OK) / 204 (No Content) if the resource has been deleted permanently (sync operation)
- In the case of async operation, the application shall return a task id that can be tracked for success/failure status.

- NOTE: You should put enough analysis in deciding the behavior when a subresource is deleted from the system. USually, you may want to **SOFT DELETE** a resource in these requests - in other words, set their status to INACTIVE.
- This allows you to not have to find and remove any references from other areas as well.

##### Applying or Removing a configuration on/from a device

- In a real application, you will need to apply the configuration on the device - OR you may want to remove the configuration from the device (not from the primary collection). You shall use PUT and DELETE methods in this case, becuase of their idempotent nature.

HTTP PUT /devices/{id}/configurations
- Apply Configuration on a device

HTTP DELETE /devices/{id}/configurations/{configId}
- Remove Configuration on a device


https://restfulapi.net/rest-api-design-tutorial-with-example/


## Glossary

### Safe Methods

- Request methods are considered *safe* if their defined semantics are essentially read-only. The client does not request nor expect any state change on the origin server as a result of applying a safe method to a target resource.
- GET, HEAD, OPTIONS and TRACE are considered to be *safe* methods. As per HTTP specification, the GET and HEAD methods should be used only for retreival of resource representations - and they do NOT update/delete the resource on the server.
- The purpose of distinguishing between safe and unsafe methods is to allow automated retrieval processes (spiders) and cache performance optimization (pre-fetching) to work without fear of causing harm.
- Safe methods allow user agents to represent other methods, such as POST, PUT and DELETE, in a unique way so that the user is made aware of the fact that possibly unsafe action is being requested - and they can update/delete the resource on the server and so should be used carefully.

### Idempotent Methods

- Idempotent is used more comprehensively to describe an **operation that will produce the same results if executed once or multiple times**
- PUT, DELETE and safe methods (GET, HEAD, OPTIONS, TRACE) are idempotent methods.
- Idempotence is a handy property in many situations, as it means that an operation can be repeated or retried as often as neccessary without causing unintended effects.
- With non-idempotent operations, the alogrithm may have to keep track of whether the operation was already performed or not.
- Like the definition of *safe methods*, the idempotent property only applies to what has been requested by the user; a server if free to log each request separately or retain a revision control history.