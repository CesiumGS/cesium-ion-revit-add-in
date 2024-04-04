cmake_minimum_required(VERSION 3.16.0...3.29.0)

if(${CMAKE_VERSION} VERSION_LESS 3.16.0)
    cmake_policy(VERSION ${CMAKE_MAJOR_VERSION}.${CMAKE_MINOR_VERSION})
endif()

set(CMAKE_SYSTEM_VERSION 10.0 CACHE STRING "" FORCE)

project(CesiumIonRevitAddin VERSION 0.1
                  DESCRIPTION "Cesium ion Revit Add-In"
                  LANGUAGES CXX)

if(NOT WIN32)
    message(FATAL_ERROR "This project is only supported on Windows.")
endif()

if(NOT CMAKE_VS_PLATFORM_NAME)
    set(CMAKE_VS_PLATFORM_NAME "x64")
endif()
message("${CMAKE_VS_PLATFORM_NAME} architecture in use")

if(NOT ("${CMAKE_VS_PLATFORM_NAME}" STREQUAL "x64"
     OR "${CMAKE_VS_PLATFORM_NAME}" STREQUAL "x86"))
    message(FATAL_ERROR "${CMAKE_VS_PLATFORM_NAME} arch is not supported!")
endif()

set(CMAKE_CONFIGURATION_TYPES
    "Debug"
    "Release"
    CACHE STRING "" FORCE
)

set_property(GLOBAL PROPERTY USE_FOLDERS ON)

add_subdirectory(extern/cesium-native EXCLUDE_FROM_ALL)
add_subdirectory(CesiumIonRevitAddin)
