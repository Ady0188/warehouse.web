using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warehouse.Web.Operations.Endpoints;

public sealed record ExportFileResult(
    byte[] Bytes,
    string FileName,
    string ContentType);
