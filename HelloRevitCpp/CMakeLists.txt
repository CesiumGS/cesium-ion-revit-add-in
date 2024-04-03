set(Header_Files
    "HelloRevitCpp.h"
    "pch.h"
    "Resource.h"
)

set(Source_Files
    "AssemblyInfo.cpp"
    "HelloRevitCpp.cpp"
    "pch.cpp"
)

set(ALL_FILES
    ${Header_Files}
    ${Source_Files}
)

set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /clr")

add_library(${PROJECT_NAME} SHARED ${ALL_FILES})

target_link_libraries(
    ${PROJECT_NAME}
    PRIVATE
        CesiumGltf
        CesiumUtility
)

set_target_properties(
    ${PROJECT_NAME}
    PROPERTIES
        COMMON_LANGUAGE_RUNTIME ""
        CXX_STANDARD 20
        CXX_STANDARD_REQUIRED YES
        CXX_EXTENSIONS NO
)

target_precompile_headers(${PROJECT_NAME} PRIVATE
    "$<$<COMPILE_LANGUAGE:CXX>:${CMAKE_CURRENT_SOURCE_DIR}/pch.h>"
)

if("${CMAKE_VS_PLATFORM_NAME}" STREQUAL "x64")
    target_compile_definitions(${PROJECT_NAME} PRIVATE
        "$<$<CONFIG:Debug>:"
            "_DEBUG"
        ">"
        "$<$<CONFIG:Release>:"
            "NDEBUG"
        ">"
        "UNICODE;"
        "_UNICODE"
    )
elseif("${CMAKE_VS_PLATFORM_NAME}" STREQUAL "x86")
    target_compile_definitions(${PROJECT_NAME} PRIVATE
        "$<$<CONFIG:Debug>:"
            "_DEBUG"
        ">"
        "$<$<CONFIG:Release>:"
            "NDEBUG"
        ">"
        "WIN32;"
        "UNICODE;"
        "_UNICODE"
    )
endif()

if(MSVC)
    target_compile_options(${PROJECT_NAME} PRIVATE
        /W3
        /EHsc
        /Zi
    )

    target_link_options(${PROJECT_NAME} PRIVATE
        /DEBUG:FULL
    )
endif()

set_property(TARGET HelloRevitCpp PROPERTY VS_DOTNET_TARGET_FRAMEWORK_VERSION "v4.8")
LIST(APPEND VS_DOTNET_REFERENCES "Microsoft.CSharp")
LIST(APPEND VS_DOTNET_REFERENCES "PresentationCore")
LIST(APPEND VS_DOTNET_REFERENCES "PresentationFramework")
LIST(APPEND VS_DOTNET_REFERENCES "System")
LIST(APPEND VS_DOTNET_REFERENCES "System.Xaml")
LIST(APPEND VS_DOTNET_REFERENCES "System.Xml")
LIST(APPEND VS_DOTNET_REFERENCES "System.Xml.Linq")
LIST(APPEND VS_DOTNET_REFERENCES "WindowsBase")
set_target_properties(HelloRevitCpp PROPERTIES
    VS_DOTNET_REFERENCES "${VS_DOTNET_REFERENCES}"
)

add_custom_command(TARGET ${PROJECT_NAME} POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_directory
            "C:/p/cesium-ion-revit-add-in/HelloRevitCpp/Images"
            "${CMAKE_BINARY_DIR}/${PROJECT_NAME}/$<CONFIG>/Images")
