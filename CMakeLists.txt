cmake_minimum_required(VERSION 3.20)

project(AppImageUpdateNET LANGUAGES C CXX)

if(CMAKE_SIZEOF_VOID_P EQUAL 8)
        # 64 bits
        set(IS32Bit FALSE)
elseif(CMAKE_SIZEOF_VOID_P EQUAL 4)
        # 32 bits
        set(IS32Bit TRUE)
endif()

set(CMAKE_INCLUDE_CURRENT_DIR ON)

set(CSharpWrapper.csprojDIR "${CMAKE_CURRENT_SOURCE_DIR}/CSharpWrapper")
set(CSharpWrapper.csproj "${CSharpWrapper.csprojDIR}/CSharpWrapper.csproj")

add_subdirectory(AppImageUpdate EXCLUDE_FROM_ALL)
get_target_property(libappimageupdate_includes libappimageupdate_static INCLUDE_DIRECTORIES)

find_package(SWIG 4.1.0 COMPONENTS csharp REQUIRED)
include(${SWIG_USE_FILE})

set_source_files_properties(src/Update.i PROPERTIES
        CPLUSPLUS ON
        INCLUDE_DIRECTORIES "${libappimageupdate_includes}"
        SWIG_OUTDIR ${CMAKE_CURRENT_BINARY_DIR}
        #SWIG_FLAGS "-includeall"
)

set_property(SOURCE src/Update.i PROPERTY COMPILE_OPTIONS
        -namespace AppImage.Update
)

# x64 lib
SWIG_ADD_LIBRARY(appimageupdate
        TYPE MODULE
        LANGUAGE csharp
        OUTPUT_DIR ${CSharpWrapper.csprojDIR}/SWIG
        SOURCES src/Update.i
)
set_target_properties(appimageupdate PROPERTIES
        LIBRARY_OUTPUT_DIRECTORY ${CSharpWrapper.csprojDIR}/runtimes/linux-x$<IF:$<BOOL:${IS32Bit}>,86,64>/native
)
target_compile_features(appimageupdate PRIVATE cxx_std_17)

SWIG_LINK_LIBRARIES(appimageupdate libappimageupdate_static)

string(TOLOWER ${CMAKE_BUILD_TYPE} MSBUILD_CONFIG)
if(MSBUILD_CONFIG STREQUAL "debug")
        set(MSBUILD_CONFIG "Debug")
else()
        set(MSBUILD_CONFIG "Release")
endif()

# build dotnet project
add_custom_target(${PROJECT_NAME} dotnet build ${CSharpWrapper.csproj} -c ${MSBUILD_CONFIG} --nologo
        COMMENT "Build ${PROJECT_NAME}..."
        DEPENDS
            appimageupdate
)

# pack dotnet project
add_custom_target(${PROJECT_NAME}.nupkg
        dotnet pack ${CSharpWrapper.csproj} -o ${CMAKE_CURRENT_BINARY_DIR} -c ${MSBUILD_CONFIG} --no-build --nologo
        COMMENT "Creating the ${PROJECT_NAME} NuGet Package"
        #${CMAKE_COMMAND} -E copy_if_different "${CSharpWrapper.csprojDIR}/bin/Release/*.nupkg" "${CMAKE_CURRENT_BINARY_DIR}"
        #COMMENT "Copy the ${PROJECT_NAME} NuGet Package ➡ ${CMAKE_CURRENT_BINARY_DIR}"
        DEPENDS
            ${PROJECT_NAME}
            TESTS
)

enable_testing()
add_subdirectory(Tests)

add_custom_target(TESTS ctest -C $<CONFIGURATION> --output-on-failure
        COMMENT "Run Tests..."
        DEPENDS
            ${PROJECT_NAME}
)

if (MSVC)
        target_compile_options(libzsync PRIVATE /W1)
        target_compile_options(libzsync2 PRIVATE /W1)
        target_compile_options(libzsync2_static PRIVATE /W1)
elseif(CMAKE_CXX_COMPILER_ID STREQUAL "GNU")
        target_compile_options(libzsync PRIVATE -w)
        target_compile_options(libzsync2 PRIVATE -w)
        target_compile_options(libzsync2_static PRIVATE -w)
else()
        target_compile_options(libzsync PRIVATE -Wno-everything)
        target_compile_options(libzsync2 PRIVATE -Wno-everything)
        target_compile_options(libzsync2_static PRIVATE -Wno-everything)
endif()
