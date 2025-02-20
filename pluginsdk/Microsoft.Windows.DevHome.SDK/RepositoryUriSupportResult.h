#pragma once
#include "RepositoryUriSupportResult.g.h"

namespace winrt::Microsoft::Windows::DevHome::SDK::implementation
{
    struct RepositoryUriSupportResult : RepositoryUriSupportResultT<RepositoryUriSupportResult>
    {
        RepositoryUriSupportResult() = default;

        RepositoryUriSupportResult(bool isSupported);
        RepositoryUriSupportResult(winrt::hresult const& e, hstring const& diagnosticText);
        bool IsSupported();
        winrt::Microsoft::Windows::DevHome::SDK::ProviderOperationResult Result();
    };
}
namespace winrt::Microsoft::Windows::DevHome::SDK::factory_implementation
{
    struct RepositoryUriSupportResult : RepositoryUriSupportResultT<RepositoryUriSupportResult, implementation::RepositoryUriSupportResult>
    {
    };
}
