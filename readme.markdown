# SkyNet

## About

SkyNet is a C# client for the Microsoft SkyDrive service through its RESTful API.  Its focus is on access to and modification of user resources -- documents, files, etc. -- and not manipulation of albums or social features such as tags and comments.

Microsoft has released the [Live SDK](http://msdn.microsoft.com/en-US/live/ff621310 "Microsoft Live SDK"), a set of C# libraries for interacting with SkyDrive, but use of these are limited to Metro and Windows Phone apps.  SkyNet was developed in response to the lack of support for server and desktop applications.

## NuGet

SkyNet is available as a [NuGet package](http://nuget.org/packages/SkyNet/).

## License & Copyright

SkyNet is Copyright 2013 The Trustees of [Indiana University](http://www.iu.edu) and is released under the [MIT License](http://opensource.org/licenses/MIT).

## Contact

You can contact me on twitter [@johnhoerr](https://twitter.com/johnhoerr).

## Release Notes

+ **1.3.1**   Fixed URL encoding issue.
+ **1.3.0**   Added support for the /quota endpoint.
+ **1.2.0**   Fixed problematic HTTP response handling.  Performance improvements.
+ **1.1.0**   Fixed folder creation bug.
+ **1.0.0**   Initial release.

## Usage

```csharp
// Instantiate a SkyNet Client.
var client = new SkyNet.Client.Client("ApiKey", "ApiSecret", "CallbackUrl", "AccessToken", "RefreshToken");

// Get the contents of the root folder
IEnumerable<File> contents = client.GetContents(Folder.Root);

// Create a new folder
Folder folder = client.CreateFolder(Folder.Root, "the folder", "the description");

// Create a new file in that folder with the content consisting of a byte array...
var content = new byte[]{1,2,3};
File file1 = client.Write(folder.Id, content, "file1", "text/plain");

// ...or a stream
File file2;
using (var stream = new MemoryStream(content))
{
  file2 = client.Write(folder.Id, stream, "file2", "text/plain");
}

// Get information about that file
File fileInfo = client.Get(file2.Id);

// Delete the folder and its contents
client.Delete(folder.Id);
```
