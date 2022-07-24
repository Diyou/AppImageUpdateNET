cmake_minimum_required(VERSION 3.20)

project(AppImageUpdateNET LANGUAGES C CXX)

set(CMAKE_INCLUDE_CURRENT_DIR ON)

set(AppImage.csprojDIR "${CMAKE_CURRENT_SOURCE_DIR}/src/AppImage")
set(AppImage.csproj "${AppImage.csprojDIR}/AppImage.csproj")

add_subdirectory(src/AppImageUpdate EXCLUDE_FROM_ALL)
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
        OUTPUT_DIR ${AppImage.csprojDIR}/Update/SWIG
        SOURCES src/Update.i
)
set_target_properties(appimageupdate PROPERTIES
        LIBRARY_OUTPUT_DIRECTORY ${AppImage.csprojDIR}/runtimes/linux-x64/native
)
target_compile_features(appimageupdate PRIVATE cxx_std_17)

SWIG_LINK_LIBRARIES(appimageupdate libappimageupdate_static)

# build dotnet project
add_custom_target(${PROJECT_NAME} dotnet build ${AppImage.csproj} -c Release --nologo
        COMMENT "Build ${PROJECT_NAME}..."
        DEPENDS
            appimageupdate
)

# pack dotnet project
add_custom_target(${PROJECT_NAME}.nupkg
        dotnet pack ${AppImage.csproj} -o ${CMAKE_CURRENT_BINARY_DIR} -c Release --no-build --nologo
        COMMENT "Creating the ${PROJECT_NAME} NuGet Package"
        #${CMAKE_COMMAND} -E copy_if_different "${AppImage.csprojDIR}/bin/Release/*.nupkg" "${CMAKE_CURRENT_BINARY_DIR}"
        #COMMENT "Copy the ${PROJECT_NAME} NuGet Package ➡ ${CMAKE_CURRENT_BINARY_DIR}"
        DEPENDS
            ${PROJECT_NAME}
            TESTS
)

enable_testing()
add_subdirectory(tests)

add_custom_target(TESTS ctest -C $<CONFIGURATION> --output-on-failure
        COMMENT "Run Tests..."
        DEPENDS
            ${PROJECT_NAME}
)

if (MSVC)
    target_compile_options(libzsync PRIVATE /W1)
    target_compile_options(libzsync2_static PRIVATE /W1)
else()
    target_compile_options(libzsync PRIVATE -Wno-everything)
    target_compile_options(libzsync2_static PRIVATE -Wno-everything)
endif()