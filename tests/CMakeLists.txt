cmake_minimum_required(VERSION 3.14.0 FATAL_ERROR)

#See Fixtures
#https://cmake.org/cmake/help/git-master/prop_test/FIXTURES_SETUP.html
set_property(GLOBAL PROPERTY CTEST_TARGETS_ADDED 1)

if(BUILD_TESTING)

file(GLOB unittests RELATIVE ${CMAKE_CURRENT_LIST_DIR} ${CMAKE_CURRENT_LIST_DIR}/*)
foreach(unittest ${unittests})
	if(IS_DIRECTORY ${CMAKE_CURRENT_LIST_DIR}/${unittest})
		add_test(NAME ${unittest}
				COMMAND dotnet test --nologo
				WORKING_DIRECTORY "${CMAKE_CURRENT_LIST_DIR}/${unittest}"
		)
		set_property(TEST ${unittest} APPEND PROPERTY LABELS DotNet)
	endif()
endforeach()

endif()