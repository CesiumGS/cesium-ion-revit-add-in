#pragma once
using namespace Autodesk::Revit::Attributes;

namespace HelloRevitCpp {

        [Transaction(TransactionMode::Manual)]
        [Regeneration(RegenerationOption::Manual)]
        public ref class Command
        : Autodesk::Revit::UI::IExternalCommand
    {
    public:
        virtual Autodesk::Revit::UI::Result Execute(
            Autodesk::Revit::UI::ExternalCommandData^ commandData,
            System::String^% message,
            Autodesk::Revit::DB::ElementSet^ elements);
    };
}