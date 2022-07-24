%module UpdateModule

%include "std_string.i"
%include "std_except.i"

%typemap(csattributes) appimage::update::Updater "[System.Runtime.Versioning.SupportedOSPlatform(\"linux\")]";
%{
    #include "appimage/update.h"
%}

%catches(std::invalid_argument,std::runtime_error) appimage::update::Updater::Updater;

%include "appimage/update.h"
