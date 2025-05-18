## Release 1.0

### New Rules

| Rule ID  | Category    | Severity | Notes                                                                      |
|----------|-------------|----------|----------------------------------------------------------------------------|
| TBTRA001 | Usage       | Error    | When using Tombatron.Results.Result<T> you must handle Ok<T> and Error<T>. |
| TBTRA002 | Usage       | Error    | When using Tombatron.Result.Result you must handle Ok and Error.           |
| TBTRA901 | Suppression | Hidden   | Suppresses CS8509 when switch expression exhaustively handles Result<T>.   | 