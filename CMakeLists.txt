cmake_minimum_required(VERSION 3.24)
project(sdi_converter VERSION 0.1.0.0 LANGUAGES CXX)
if(NOT MSVC OR NOT CMAKE_SIZEOF_VOID_P EQUAL 8)
  message(FATAL_ERROR "Windows MSVC x64 required")
endif()

# Blackmagic Desktop Video 14 installs into "Blackmagic Desktop Video", newer versions into "Desktop Video".
find_file(DECKLINK_API_DLL DeckLinkAPI64.dll NO_DEFAULT_PATH PATHS
  "C:/Program Files/Blackmagic Design/Desktop Video"
  "C:/Program Files/Blackmagic Design/Blackmagic Desktop Video")
if(NOT DECKLINK_API_DLL)
  message(FATAL_ERROR "DeckLinkAPI64.dll not found: install Blackmagic Desktop Video")
endif()
message(STATUS "DeckLink API: ${DECKLINK_API_DLL}")
# Desktop Video 14.3+ keeps the old API as *_v14_2_1; older drivers (e.g. 12.x) only know the plain names.
execute_process(COMMAND powershell -NoProfile -Command
  "if ([Text.Encoding]::ASCII.GetString([IO.File]::ReadAllBytes('${DECKLINK_API_DLL}')).Contains('IDeckLinkInput_v14_2_1')) { 'ON' } else { 'OFF' }"
  OUTPUT_VARIABLE DECKLINK_HAS_V14_2_1 OUTPUT_STRIP_TRAILING_WHITESPACE)
message(STATUS "DeckLink *_v14_2_1 interfaces: ${DECKLINK_HAS_V14_2_1}")
if(DECKLINK_HAS_V14_2_1)
  set(DECKLINK_COMPAT "#define SDI_DECKLINK_V14_2_1 1")
endif()
configure_file(src/decklink_import.h.in "${CMAKE_BINARY_DIR}/generated/decklink_import.h" @ONLY)

set(OMT_DIR "${CMAKE_CURRENT_SOURCE_DIR}/third_party/omt/Libraries/Winx64")
add_executable(engine src/main.cpp)
set_target_properties(engine PROPERTIES OUTPUT_NAME sdi_converter_engine)
target_compile_features(engine PRIVATE cxx_std_20)
target_compile_definitions(engine PRIVATE NOMINMAX WIN32_LEAN_AND_MEAN)
target_compile_options(engine PRIVATE /W4 /utf-8 /EHsc)
target_include_directories(engine PRIVATE "${OMT_DIR}" "${CMAKE_BINARY_DIR}/generated" src)
target_link_directories(engine PRIVATE "${OMT_DIR}")
target_link_libraries(engine PRIVATE libomt ole32 oleaut32)
foreach(dll libomt libvmx)
  add_custom_command(TARGET engine POST_BUILD COMMAND ${CMAKE_COMMAND} -E copy_if_different
    "${OMT_DIR}/${dll}.dll" "$<TARGET_FILE_DIR:engine>")
endforeach()
