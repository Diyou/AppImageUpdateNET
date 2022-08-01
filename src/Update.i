%module UpdateModule

%include "std_string.i"
%include "std_except.i"
%include "typemaps.i"

%include "std_string_ref.i"

%inline %{
    typedef long off_t;
%}

//%rename("%(regex:/(([a-zA-Z])((?<=[a-zA-Z])[a-zA-Z]*))([_-]*)?/\\U\\2\\L\\3/)s",regexmatch$parentNode$type="enum .*") "";
//%rename("%(lowercase)s",regexmatch$parentNode$type="enum .*") "";
//%rename("%(camelcase)s",regexmatch$parentNode$type="enum .*") "";

namespace appimage{
    namespace update{

        %typemap(csattributes) Updater "[System.Runtime.Versioning.SupportedOSPlatform(\"linux\")]";
        %{
            #include "appimage/update.h"
        %}

        %catches(std::invalid_argument,std::runtime_error) Updater::Updater;
        %catches(std::runtime_error) Updater::checkForChanges;
        %catches(std::runtime_error) Updater::start;

        %apply double& INOUT { double& progress};
        bool Updater::progress(double& progress);

        %apply bool& INOUT { bool& updateAvailable};
        bool Updater::checkForChanges(bool& updateAvailable, unsigned int method);

        %apply long& INOUT { long& fileSize};
        bool Updater::remoteFileSize(long& fileSize);
    }
}

%include "appimage/update.h"
