# Unreal Engine 4 Protobuf Example

## Overview

The directory `Source/ThirdParty/protobuf` contains the includes and libraries for protobuf (just copied) as well as a build script that adds these and also defined `GOOGLE_PROTOBUF_NO_RTTI=1` to disable RTTI. `Source/ProtobufExample` contains the generated code based on the [address book example](https://github.com/protocolbuffers/protobuf/blob/master/examples/addressbook.proto). The generated c++ file had some warnings disabled to make it compile as UE4 treats these as errors. 

`Source/ProtobufExample/ProtobufExampleGameModeBase.cpp` contains code making use of the address book class.

## Building protobuf
### Building on Windows
To build the protobuf C++ binaries, follow the official instructions for using `vcpkg` on Windows: https://github.com/protocolbuffers/protobuf/tree/master/src#c-installation---windows

### Building on Ubuntu Linux
Before building protobuf on Ubuntu, you will need to build UE4 as described in the [Linux Quick Start](https://docs.unrealengine.com/en-US/Platforms/Linux/BeginnerLinuxDeveloper/SettingUpAnUnrealWorkflow/index.html).

On Linux, UE4 uses a nonstandard toolchain installed by the `./Setup.sh` script described in section 3 -- "Building UE4 on Linux" -- of the [Linux Quick Start](https://docs.unrealengine.com/en-US/Platforms/Linux/BeginnerLinuxDeveloper/SettingUpAnUnrealWorkflow/index.html). In addition, `UnrealBuildTool` silently uses its own STL headers and libraries that are incompatible with the default used by clang or gcc, which leads to indecipherable link errors that I was only able to resolve by meticulously configuring the protobuf build to look for all its headers and libraries in the same locations that UBT is secretly using.

To compile protobuf on Linux, follow the instructions in the protobuf documentation for [C++ Installation - Unix](https://github.com/protocolbuffers/protobuf/tree/master/src#c-installation---unix). When you arrive at the step "execute the following", you must pass a bunch of extra arguments to `.configure`. These arguments ensure that protobuf is compiled with the same libraries and header files that are used by UrealBuildTool to compile plugins and modules. (Note that the UE_TOOLCHAIN location depends on the version of Unreal Engine. ["Cross Compiling for Linux"](https://docs.unrealengine.com/en-US/Platforms/Linux/GettingStarted/index.html) indicates which toolchain is used for each engine version.):
```
export PROJECT_DIR=<the directory where you cloned UE4ProtobufExample>
export UNREAL_ENGINE_DIR=<the directory where you cloned UnrealEngine>
export UE_TOOLCHAIN=$UNREAL_ENGINE_DIR/Engine/Extras/ThirdPartyNotUE/SDKs/HostLinux/Linux_x64/v15_clang-8.0.1-centos7/x86_64-unknown-linux-gnu
./configure \
    --prefix=$PROJECT_DIR/UE4ProtobufExample/Source/ThirdParty/protobuf \
    --libdir=$PROJECT_DIR/UE4ProtobufExample/Source/ThirdParty/protobuf/lib/Linux/ \
    --with-pic \
    --with-sysroot=$UE_TOOLCHAIN \
    "CC=$UE_TOOLCHAIN/bin/clang" \
    "CXX=$UE_TOOLCHAIN/bin/clang++" \
    "CPPFLAGS=--sysroot=$UE_TOOLCHAIN -B$UE_TOOLCHAIN/usr/lib64 -iwithsysroot/../../../../../../../Source/ThirdParty/Linux/LibCxx/include -iwithsysroot/../../../../../../../Source/ThirdParty/Linux/LibCxx/include/c++/v1 -iwithsysroot/lib/clang/8.0.1/include -iwithsysroot/include -iwithsysroot/usr/include -iwithsysroot/usr/include/linux -fno-exceptions" \
    "CXXFLAGS=-std=c++14 -gdwarf-4 -DNDEBUG -fno-rtti" \
    "CXXFLAGS_FOR_BUILD=-nostdinc++" \
    "LDFLAGS=--sysroot=$UE_TOOLCHAIN -B$UE_TOOLCHAIN/usr/lib64 -L$UNREAL_ENGINE_DIR/Engine/Source/ThirdParty/Linux/LibCxx/lib/Linux/x86_64-unknown-linux-gnu -stdlib=libc++"
make -j32
make check -j32
make install
```

`make -j32` will compiles protobuf. `make check` runs some tests to ensure that protobuf is functioning on your platform. `make install` will copy 

Finally, compile the `UE4ProtobufExample` project:
```
 $UNREAL_ENGINE_DIR/Engine/Build/BatchFiles/Linux/Build.sh ProtobufExampleEditor Linux Development "$PROJECT_DIR/UE4ProtobufExample/ProtobufExample.uproject" -buildscw
```

### Troubleshooting Linux Compilation
If you encounter issues compiling protobuf or the sample project on Linux, I feel bad for you son. Here are some tips:

#### ./configure Errors
The `./configure ... ` command pounded me with a litany of seemingly nonsensical complaints. When `./configure` fails, it generates a log files `config.log` in the same directory where `./configure` was run. This log contains compile errors or other diagnostic information that can be used to diagnose errors.

#### Understand ./configure Arguments
Each argument to `./configure` was discovered via much trial, error, and gnashing of teeth. Run `./configure --help` for a cursory overview of the options available. Here's an explainer:
* `--prefix=$PROJECT_DIR/UE4ProtobufExample/Source/ThirdParty/protobuf` tells `make install` to copy `protoc` to the project dir.
* `--libdir=$PROJECT_DIR/UE4ProtobufExample/Source/ThirdParty/protobuf/lib/Linux/` tells `make install` to copy libs and shared libs (`.a` files, `.so` files, etc.) to the `lib/Linux` subdirectory.
* `--with-pic` tell the compiler to generate "Position Independent Code", which is required by UE4. Without this argument, the `ProtobufExample` project fails to link.
* `--with-sysroot=$UE_TOOLCHAIN` tell the compiler and linker where to find the root directory of the toolchain. This doesn't seem to work. I was forced to also pass `--sysroot` arguments in `LDFLAGS` and `CPPFLAGS`.
* `"CC=$UE_TOOLCHAIN/bin/clang"` use UE4's version of clang to compile `.cpp` files. 
* `"CXX=$UE_TOOLCHAIN/bin/clang++"` use UE4's version of clang to compile `.c` files. 
* `"CPPFLAGS=` extra flags passed to both the C and  C++ compilers...
* `--sysroot=$UE_TOOLCHAIN` the root directory of the toolchain
* `-iwithsysroot...` standard include directories relative to the `sysroot` directory
* `-B$UE_TOOLCHAIN/usr/lib64` tell the compiler where to find `crt1.o` and its brethren. 
* `-fno-exceptions` don't compile with exception handling. Without this flag, `protoc` will fail to link with a complaint about not finding `std::logic_error`.
* `"CXXFLAGS=` extra flags passed to the C++ compiler...
* `-std=c++14 -gdwarf-4 -fno-rtti` the C++ version, debug data format, and RTTI settings used by UE4 as of UE 4.23
* `"CXXFLAGS_FOR_BUILD=-nostdinc++"` ignore the system default include directories. The standard headers are incompatible with the special STL headers used by UnrealBuildTool.
* `"LDFLAGS=` extra flags passed to the linker...
* `--sysroot=$UE_TOOLCHAIN` root directory of the toolchain... again.
* `-B$UE_TOOLCHAIN/usr/lib64` tell the compiler where to find `crt1.o`. Without this flag, protobuf will fail to link because the standard search path for built-in `.o` files is not the same as the layout in UE4's toolchain.
* `-L$UNREAL_ENGINE_DIR/Engine/Source/ThirdParty/Linux/LibCxx/lib/Linux/x86_64-unknown-linux-gnu` the location of `libc++.a` containing the standard C++ library. It is not compatible with the one that comes with the toolchain. Without this argument, protobuf will fail to link.
* `-stdlib=libc++` Use `libc++.a` instead of the default `libstdc++.a`

#### Examine Compile/Link Args in .rsp and .response Files
UnrealBuildTool generates `.rsp` and `.response` files that show the arguments passed to the compiler and linker. You might find clues that point to a library or header file being used by UE4 that conflicts with one used by protobuf. 

For example, the file `$PROJECT_DIR/UE4ProtobufExample/Intermediate/Build/Linux/B4D820EA/UE4Editor/Development/ProtobufExample/addressbook.pb.cc.o.rsp` contains compile arguments used by UBT. In this file I noticed that UBT passes an include directory `-IThirdParty/Linux/LibCxx/include/c++/v1` that contains non-standard definitions of STL types (string, iostream, etc.) used by protobuf. These types are incompatible with the default headers used by `clang` or `gcc`. Also, the very first argument to the compiler is `--sysroot="..."` which you can use to confirm the location of the toolchain used by UE4.


## Generating C++ Source
XXXdsmith: I built this project with UE 4.23.1. To generate source with protoc:
```
 C:\proj\unreal\UE4ProtobufExample\Source\ThirdParty\protobuf\tools\protobuf\protoc.exe -I=C:\proj\unreal\vcpkg\buildtrees\protobuf\src\v3.11.3-464ce72ac5\src\ -I=C:\proj\unreal\vcpkg\buildtrees\protobuf\src\v3.11.3-464ce72ac5\examples\ --cpp_out=C:\proj\unreal\UE4ProtobufExample\Source\ProtobufExample\ C:\proj\unreal\vcpkg\buildtrees\protobuf\src\v3.11.3-464ce72ac5\examples\addressbook.proto
```

You must manually edit the generated source code to make it compile successfully.
```
// xxxdsmith this is the only warning I had to disable. This is not necessary on Linux
#pragma warning(disable: 4125) // decimal digit terminates octal escape sequence
#pragma warning(disable: 4668) // xxxdsmith I didn't have to do this
#pragma warning(disable: 4577) // xxxdsmith I didn't have to do this

// xxxdsmith in the generated header addressbook.pb.h, I had to rename MOBILE to _MOBILE to avoid a compile error due to a collision with a built-in UE4 name:
constexpr Person_PhoneType Person::_MOBILE; // instead of ::MOBILE
```
In UE4 versions prior to 4.22, you must rename the generated protobuf source file extensions from .cc to .cpp (e.g. "addressbook.cc" must be renamed to "addressbook.cpp").

