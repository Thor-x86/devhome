#pragma once
#include "DeveloperIdResult.g.h"

namespace winrt::Microsoft::Windows::DevHome::SDK::implementation
{
    struct DeveloperIdResult : DeveloperIdResultT<DeveloperIdResult>
    {
        DeveloperIdResult() = default;

        DeveloperIdResult(winrt::Microsoft::Windows::DevHome::SDK::IDeveloperId const& developerId);
        DeveloperIdResult(winrt::hresult const& e, hstring const& diagnosticText);
        winrt::Microsoft::Windows::DevHome::SDK::IDeveloperId DeveloperId();
        winrt::Microsoft::Windows::DevHome::SDK::ProviderOperationResult Result();
    };
}
namespace winrt::Microsoft::Windows::DevHome::SDK::factory_implementation
{
    struct DeveloperIdResult : DeveloperIdResultT<DeveloperIdResult, implementation::DeveloperIdResult>
    {
    };
}
