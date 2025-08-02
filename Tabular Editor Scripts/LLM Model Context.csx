using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

/* ── EXPORT SWITCHES ─────────────────────────────────────────── */
bool EXPORT_TABLES         = true;
bool EXPORT_RELATIONSHIPS  = true;
bool EXPORT_MEASURES       = true;
/* ────────────────────────────────────────────────────────────── */

// ---------- helpers ----------
bool IsCalculatedTable(Table t) =>
    t.Partitions.Count == 1 &&
    t.Partitions[0].SourceType == PartitionSourceType.Calculated;

string GetDynamicFormatExpression(Measure m)
{
    var fsProp = m.GetType().GetProperty("FormatStringDefinition");
    if (fsProp == null) return null;

    var fsDef = fsProp.GetValue(m, null);
    if (fsDef == null) return null;

    var exprProp = fsDef.GetType().GetProperty("Expression");
    return exprProp?.GetValue(fsDef, null) as string;
}

// ---------- tables ----------
var tables = EXPORT_TABLES
    ? Model.Tables.Select(t => new {
        name         = t.Name,
        isHidden     = t.IsHidden,
        isCalculated = IsCalculatedTable(t),
        columns      = t.Columns.Select(c => new {
                          name      = c.Name,
                          dataType  = c.DataType.ToString(),
                          isHidden  = c.IsHidden
                      })
      })
    : null;

// ---------- relationships ----------
var rels = EXPORT_RELATIONSHIPS
    ? Model.Relationships
        .OfType<SingleColumnRelationship>()
        .Select(r => new {
            from        = $"{r.FromTable.Name}.{r.FromColumn.Name}",
            to          = $"{r.ToTable.Name}.{r.ToColumn.Name}",
            cardinality = $"{r.FromCardinality}-{r.ToCardinality}",
            isActive    = r.IsActive
        })
    : null;

// ---------- measures ----------
var meas = EXPORT_MEASURES
    ? Model.AllMeasures.Select(m => new {
        name                    = m.Name,
        expression              = m.Expression,
        formatString            = m.FormatString,
        dynamicFormatExpression = GetDynamicFormatExpression(m)
      })
    : null;

// ---------- payload ----------
var payload = new System.Collections.Generic.Dictionary<string, object>();
if (tables != null) payload["tables"]        = tables;
if (rels   != null) payload["relationships"] = rels;
if (meas   != null) payload["measures"]      = meas;

// ---------- wrap with model header ----------
var root = new System.Collections.Generic.Dictionary<string, object>();
root["modelInfo"] = "Power BI Semantic Model";
foreach (var kv in payload) root[kv.Key] = kv.Value;

// ---------- write file ----------
var dir = Path.Combine(
    System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments),
    "LLM");
Directory.CreateDirectory(dir);

var outPath = Path.Combine(dir, "context.json");
File.WriteAllText(outPath,
    JsonConvert.SerializeObject(root, Formatting.Indented));

Output($"✅  Context file written to: {outPath}");
