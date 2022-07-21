%module UpdateModule

%include "std_string.i"
%include "std_except.i"

%typemap(csattributes) appimage::update::Updater "[System.Runtime.Versioning.SupportedOSPlatform(\"linux\")]";
%{
    #include "appimage/update.h"
%}

%catches(std::invalid_argument) appimage::update::Updater::Updater;
%catches(std::invalid_argument) appimage::update::Updater::testExec;

%include "appimage/update.h"
