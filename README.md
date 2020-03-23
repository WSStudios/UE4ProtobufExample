# Unreal Engine 4 Protobuf Example

## Overview

The directory `Source/ThirdParty/protobuf` contains the includes and libraries for protobuf (just copied) as well as a build script that adds these and also defined `GOOGLE_PROTOBUF_NO_RTTI=1` to disable RTTI. `Source/ProtobufExample` contains the generated code based on the [address book example](https://github.com/protocolbuffers/protobuf/blob/master/examples/addressbook.proto). The generated c++ file had some warnings disabled to make it compile as UE4 treats these as errors. 

To build the protobuf C++ binaries, follow the official instructions for using `vcpkg` on Windows: https://github.com/protocolbuffers/protobuf/tree/master/src#c-installation---windows

XXXdsmith: I built this project with UE 4.23.1. To generate source with protoc:
```
 C:\proj\unreal\UE4ProtobufExample\Source\ThirdParty\protobuf\tools\protobuf\protoc.exe -I=C:\proj\unreal\vcpkg\buildtrees\protobuf\src\v3.11.3-464ce72ac5\src\ -I=C:\proj\unreal\vcpkg\buildtrees\protobuf\src\v3.11.3-464ce72ac5\examples\ --cpp_out=C:\proj\unreal\UE4ProtobufExample\Source\ProtobufExample\ C:\proj\unreal\vcpkg\buildtrees\protobuf\src\v3.11.3-464ce72ac5\examples\addressbook.proto
```

```
#pragma warning(disable: 4125) // xxxdsmith this is the only warning I had to disable
#pragma warning(disable: 4668) // xxxdsmith I didn't have to do this
#pragma warning(disable: 4577) // xxxdsmith I didn't have to do this

// xxxdsmith in the generated header addressbook.pb.h, I had to rename MOBILE to _MOBILE to avoid a compile error due to a collision with a built-in UE4 name:
constexpr Person_PhoneType Person::_MOBILE; // instead of ::MOBILE
```

Furthermore because of a bug in UE4 4.21 the source file had its extension renamed from .cc to .cpp, although this is not necessary anymore in 4.22. `Source/ProtobufExample/ProtobufExampleGameModeBase.cpp` contains code making use of the address book class.

## Remarks
Currently only contains binaries for Windows 64 bit.