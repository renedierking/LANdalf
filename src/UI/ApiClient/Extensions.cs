namespace LANdalf.UI.ApiClient {
    public static class Extensions {
        public static PcDeviceDTO Clone(this PcDeviceDTO dTO) {
            return new PcDeviceDTO {
                Id = dTO.Id,
                Name = dTO.Name,
                MacAddress = dTO.MacAddress,
                IpAddress = dTO.IpAddress,
                BroadcastAddress = dTO.BroadcastAddress,
                IsOnline = dTO.IsOnline,
                OnlineSince = dTO.OnlineSince,
                GroupName = dTO.GroupName
            };
        }
    }
}
