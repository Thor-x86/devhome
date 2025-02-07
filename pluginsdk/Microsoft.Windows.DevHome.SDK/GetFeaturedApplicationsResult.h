#pragma once
#include "GetFeaturedApplicationsResult.g.h"

namespace winrt::Microsoft::Windows::DevHome::SDK::implementation
{
    struct GetFeaturedApplicationsResult : GetFeaturedApplicationsResultT<GetFeaturedApplicationsResult>
    {
        GetFeaturedApplicationsResult() = default;

        GetFeaturedApplicationsResult(winrt::Windows::Foundation::Collections::IVectorView<hstring> const& featuredApplications);
        GetFeaturedApplicationsResult(winrt::hresult const& e, hstring const& diagnosticText);
        winrt::Windows::Foundation::Collections::IVectorView<hstring> FeaturedApplications();
        winrt::Microsoft::Windows::DevHome::SDK::ProviderOperationResult Result();
    };
}
namespace winrt::Microsoft::Windows::DevHome::SDK::factory_implementation
{
    struct GetFeaturedApplicationsResult : GetFeaturedApplicationsResultT<GetFeaturedApplicationsResult, implementation::GetFeaturedApplicationsResult>
    {
    };
}
