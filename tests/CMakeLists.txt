cmake_minimum_required(VERSION 3.14.0 FATAL_ERROR)

#See Fixtures
#https://cmake.org/cmake/help/git-master/prop_test/FIXTURES_SETUP.html
set_property(GLOBAL PROPERTY CTEST_TARGETS_ADDED 1)

if(BUILD_TESTING)

file(GLOB unittests RELATIVE ${CMAKE_CURRENT_LIST_DIR} ${CMAKE_CURRENT_LIST_DIR}/*)
foreach(unittest ${unittests})
	if(IS_DIRECTORY ${CMAKE_CURRENT_LIST_DIR}/${unittest})
		add_test(NAME ${unittest} COMMAND
				#${CMAKE_COMMAND} -E env
				#	LD_LIBRARY_PATH=runtimes/linux-x64/native
				dotnet test --filter FullyQualifiedName~${unittest} -c ${MSBUILD_CONFIG} --nologo
				WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
		)
		set_property(TEST ${unittest} APPEND PROPERTY LABELS DotNet)
	endif()
endforeach()

endif()