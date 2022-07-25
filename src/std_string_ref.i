// string &

%typemap(ctype) std::string & "char**"
%typemap(imtype) std::string & "/*imtype*/ ref string"
%typemap(cstype) std::string & "/*cstype*/ ref string"

//C++
%typemap(in, canthrow=1) std::string &
%{  //typemap in
std::string temp;
$1 = &temp;
%}

//C++
%typemap(argout) std::string &
%{
//Typemap argout in c++ file.
//This will convert c++ string to c# string
*$input = SWIG_csharp_string_callback($1->c_str());
%}

%typemap(argout) const std::string &
%{
//argout typemap for const std::string&
%}

%typemap(csin) std::string & "ref $csinput"

%typemap(throws, canthrow=1) std::string &
%{
SWIG_CSharpSetPendingException(SWIG_CSharpApplicationException, $1.c_str());
return $null;
%}
