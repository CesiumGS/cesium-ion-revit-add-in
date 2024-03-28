// #include "stdafx.h"
#include "pch.h"
#include "HelloRevitCpp.h"

using namespace System;
using namespace Autodesk::Revit::ApplicationServices;
using namespace Autodesk::Revit::Attributes;
using namespace Autodesk::Revit::DB;
using namespace Autodesk::Revit::UI;

using namespace HelloRevitCpp;

//Result Command::Execute(
//	ExternalCommandData^ commandData,
//	String^% message,
//	ElementSet^ elements)
//{
//	TaskDialog::Show("Revit", "Using C++/CLI doesn't seem too bad");
//	return Autodesk::Revit::UI::Result::Succeeded;
//}

Autodesk::Revit::UI::Result HelloRevitCpp::ExternalApplication::OnStartup(UIControlledApplication^ application)
{
	return Autodesk::Revit::UI::Result::Succeeded;
}

Autodesk::Revit::UI::Result HelloRevitCpp::ExternalApplication::OnShutdown(UIControlledApplication^ application)
{
	return Autodesk::Revit::UI::Result::Succeeded;
}
