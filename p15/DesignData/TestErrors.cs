using p15.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;

namespace p15.DesignData
{
    public class TestErrors : ReactiveObject
    {
        public IEnumerable<ErrorViewModel> Errors => new[]
        {
            new ErrorViewModel
            {
                ApplicationName = "Company.AppName.DatabaseUpdater",
                Timestamp = DateTime.Parse("21 Mar 2020 19:00:00.123"),
                Error = "It's possible for RavenDB to lose data when used with Distributed Transaction Coordinator (DTC) transactions. Future versions of NServiceBus RavenDB Persistence will not support this combination. If using the same RavenDB database for NServiceBus data and all business data, you can disable enlisting by calling Transactions().DisableDistributedTransactions() on the BusConfiguration instance and enable the Outbox feature to maintain consistency between messaging operations and data persistence. See 'DTC not supported for RavenDB Persistence' in the documentation for more details."
            },
            new ErrorViewModel
            {
                ApplicationName = "Company.AppName.API",
                Timestamp = DateTime.Parse("21 Mar 2020 19:02:03.567"),
                Error = "There is an error in Company.AppName.API\n1\n2\n3\n4\n5\n6\n7\n8\n9\n10\n11\n12\n13\n15\n16\n17\n18\n19\n20\nSo there!"
            }
        };
    }
}
