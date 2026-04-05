using System;
using System.Collections.Generic;

namespace PublishRealLiteApi.DTOs
{
    public class StatsSummaryDto
    {
        public long TotalStreams { get; set; }
        public IEnumerable<ByDateDto> ByDate { get; set; } = Array.Empty<ByDateDto>();
        public IEnumerable<ByCountryDto> ByCountry { get; set; } = Array.Empty<ByCountryDto>();
        public IEnumerable<BySourceDto> BySource { get; set; } = Array.Empty<BySourceDto>();
    }
    public class ByDateDto { public DateTime Date { get; set; } public int Streams { get; set; } }
    public class ByCountryDto { public string Country { get; set; } = string.Empty; public int Streams { get; set; } }
    public class BySourceDto { public string Source { get; set; } = string.Empty; public int Streams { get; set; } }

}
