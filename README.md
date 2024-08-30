# Mocca

A C# web server for mocking web APIs. One web server records API request and stores the responses in a file. The other web server uses this file to replay responses.

## Configuration 

```jsonc
{
    // The Mocca configuration section
    "Mocca": {
        // The URL of the web API that you want to mock.
        "ForwardTo": "https://localhost:7228",
        // The file where you want to store the responses in.
        "ResponseFile": "../responses.txt",
        // The request paths that should not be stored in the file. 
        "IgnoredPaths": [
            "/swagger**"
        ],
        // The JSON properties you want to overwrite.
        "Overwrite": [
            {
                // The method that the request should have.
                "RequestMethod": "GET",
                // The path pattern that the request should match.
                "RequestPathPattern": "/weatherforecast",
                // The JSON property path.
                "PropertyPath": "[4].temperatureC",
                // The replacement value.
                "Value": "69"
            },
            // Add more properties you want to replace.
        ]
    }
}
```
