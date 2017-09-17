#I __SOURCE_DIRECTORY__
#r "../packages/Akka.1.1.3/lib/net45/Akka.dll"
#r "../packages/Akka.FSharp.1.1.3/lib/net45/Akka.FSharp.dll"
#r "../packages/FsPickler.1.2.21/lib/net45/FsPickler.dll"
#r "../packages/Newtonsoft.Json.10.0.3/lib/net45/Newtonsoft.Json.dll"
#r "../packages/System.Collections.Immutable.1.1.36/lib/portable-net45+win8+wp8+wpa81/System.Collections.Immutable.dll"

//#r "../packages/Akka.Persistence/lib/net45/Akka.Persistence.dll"
//#r "../packages/Akka.Persistence.FSharp/lib/net45/Akka.Persistence.FSharp.dll"
//#r "packages/Google.ProtocolBuffers/lib/net40/Google.ProtocolBuffers.dll"
//#r "packages/Google.ProtocolBuffers/lib/net40/Google.ProtocolBuffers.Serialization.dll"

#r "../Actionable.Domain/bin/Debug/Actionable.Domain.dll"
#r "../Actionable.Domain.Persistance/bin/Debug/Actionable.Domain.Persistance.dll"
#r "../Actionable.Data/bin/Debug/Actionable.Data.dll"

#r "../packages/EntityFramework.6.1.3/lib/net45/EntityFramework.dll"
#r "../packages/EntityFramework.6.1.3/lib/net45/EntityFramework.SqlServer.dll"
//#r "../packages/Akka.Persistence/lib/net45/Akka.Persistence.dll"
//#r "../packages/Akka.Persistence.FSharp/lib/net45/Akka.Persistence.FSharp.dll"
//#r "../packages/Google.ProtocolBuffers/lib/net40/Google.ProtocolBuffers.dll"
//#r "../packages/Google.ProtocolBuffers/lib/net40/Google.ProtocolBuffers.Serialization.dll"

#load "../Actionable.Actors/Infrastructure.fs"
#load "../Actionable.Actors/Aggregates.fs"
#load "../Actionable.Actors/Persistance.fs"
#load "../Actionable.Actors/Initialization.fs"
#load "../Actionable.Actors/NewtonsoftHack.fs"
#load "../Actionable.Actors/Composition.fs"


