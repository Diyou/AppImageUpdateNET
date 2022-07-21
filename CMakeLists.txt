cmake_minimum_required(VERSION 3.22)

project(AppImageUpdateNET LANGUAGES C CXX)

set(CMAKE_INCLUDE_CURRENT_DIR ON)
set(CMAKE_MODULE_PATH
        ${CMAKE_CURRENT_SOURCE_DIR}/tests;
)

add_subdirectory(submodules/AppImageUpdate EXCLUDE_FROM_ALL)
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
        TYPE SHARED
        LANGUAGE csharp
        OUTPUT_DIR ${CMAKE_CURRENT_SOURCE_DIR}/AppImage/Update
        SOURCES src/Update.i
)
set_target_properties(appimageupdate PROPERTIES
        LIBRARY_OUTPUT_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}/AppImage/runtimes/linux-x64/native
)
target_compile_features(appimageupdate PRIVATE cxx_std_17)
SWIG_LINK_LIBRARIES(appimageupdate libappimageupdate_static)

# pack dotnet project
add_custom_target(${PROJECT_NAME}.nupkg
        dotnet pack ${CMAKE_CURRENT_SOURCE_DIR}/AppImage/AppImage.csproj -o ${CMAKE_CURRENT_BINARY_DIR} -c Release --nologo
        COMMENT "Creating the ${PROJECT_NAME} NuGet Package"
)

enable_testing()
add_subdirectory(tests)

add_custom_target(TESTS ctest -C $<CONFIGURATION> --output-on-failure
        DEPENDS appimageupdate
        COMMENT "Run Tests..."
)

add_dependencies(${PROJECT_NAME}.nupkg appimageupdate TESTS)

if (MSVC)
    target_compile_options(libzsync PRIVATE /W1)
    target_compile_options(libzsync2_static PRIVATE /W1)
else()
    target_compile_options(libzsync PRIVATE -Wno-everything)
    target_compile_options(libzsync2_static PRIVATE -Wno-everything)
endif()