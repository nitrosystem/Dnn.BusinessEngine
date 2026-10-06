export const SQL_TYPE_REGEX = /^([a-z_]+)(?:\((\d+|max)(?:\s*,\s*(\d+))?\))?$/i;

export const SQL_TYPES = Object.freeze({
    noLength: new Set([
        'int', 'bigint', 'smallint', 'tinyint', 'bit',
        'float', 'real', 'money', 'smallmoney',
        'date', 'datetime', 'smalldatetime', 'timestamp',
        'geography', 'geometry', 'hierarchyid',
        'image', 'text', 'ntext',
        'sql_variant', 'uniqueidentifier', 'xml'
    ]),
    singleLength: new Set([
        'binary', 'varbinary', 'char', 'varchar',
        'nvarchar', 'nchar', 'datetime2', 'datetimeoffset', 'time'
    ]),
    precisionScale: new Set(['decimal', 'numeric'])
});

export const SQL_TYPE_MAP = [
    { sql: /^bigint$/i, cs: "long?" },
    { sql: /^binary|varbinary/i, cs: "byte[]" },
    { sql: /^bit$/i, cs: "bool?" },
    { sql: /^char|nchar|varchar|nvarchar|text|ntext/i, cs: "string" },
    { sql: /^date$/i, cs: "DateTime?" },
    { sql: /^datetime$/i, cs: "DateTime?" },
    { sql: /^datetime2$/i, cs: "DateTime?" },
    { sql: /^datetimeoffset$/i, cs: "DateTimeOffset" },
    { sql: /^decimal|numeric/i, cs: "decimal?" },
    { sql: /^float$/i, cs: "double?" },
    { sql: /^int$/i, cs: "int?" },
    { sql: /^money|smallmoney$/i, cs: "decimal?" },
    { sql: /^real$/i, cs: "float?" },
    { sql: /^smallint$/i, cs: "short?" },
    { sql: /^small datetime$/i, cs: "DateTime?" },
    { sql: /^time$/i, cs: "TimeSpan" },
    { sql: /^tinyint$/i, cs: "byte?" },
    { sql: /^uniqueidentifier$/i, cs: "Guid?" },
    { sql: /^xml$/i, cs: "string" },
    { sql: /^image$/i, cs: "byte[]" },
    { sql: /^sql_variant$/i, cs: "object" }
];

export const SQL_ALIAS_RESERVED_WORDS = new Set([
    'add', 'all', 'alter', 'and', 'any', 'as', 'asc', 'authorization',
    'backup', 'begin', 'between', 'break', 'browse', 'bulk', 'by',
    'cascade', 'case', 'check', 'checkpoint', 'close', 'clustered',
    'coalesce', 'collate', 'column', 'commit', 'compute', 'constraint',
    'contains', 'containstable', 'continue', 'convert', 'create', 'cross',
    'current', 'current_date', 'current_time', 'current_timestamp',
    'current_user', 'cursor', 'database', 'dbcc', 'deallocate', 'declare',
    'default', 'delete', 'deny', 'desc', 'disk', 'distinct', 'distributed',
    'double', 'drop', 'dump', 'else', 'end', 'errlvl', 'escape', 'except',
    'exec', 'execute', 'exists', 'exit', 'external', 'fetch', 'file',
    'fillfactor', 'for', 'foreign', 'freetext', 'freetexttable', 'from',
    'full', 'function', 'goto', 'grant', 'group', 'having', 'holdlock',
    'identity', 'identity_insert', 'identitycol', 'if', 'in', 'index',
    'inner', 'insert', 'intersect', 'into', 'is', 'join', 'key', 'kill',
    'left', 'like', 'lineno', 'load', 'merge', 'national', 'nocheck',
    'nonclustered', 'not', 'null', 'nullif', 'of', 'off', 'offsets',
    'on', 'open', 'opendatasource', 'openquery', 'openrowset', 'openxml',
    'option', 'or', 'order', 'outer', 'over', 'percent', 'pivot', 'plan',
    'precision', 'primary', 'print', 'proc', 'procedure', 'public',
    'raiserror', 'read', 'readtext', 'reconfigure', 'references',
    'replication', 'restore', 'restrict', 'return', 'revert', 'revoke',
    'right', 'rollback', 'rowcount', 'rowguidcol', 'rule', 'save',
    'schema', 'securityaudit', 'select', 'semantickeyphrasetable',
    'semanticsimilaritydetailstable', 'semanticsimilaritytable',
    'session_user', 'set', 'setuser', 'shutdown', 'some', 'statistics',
    'system_user', 'table', 'tablesample', 'textsize', 'then', 'to',
    'top', 'tran', 'transaction', 'trigger', 'truncate', 'try_convert',
    'tsequal', 'union', 'unique', 'unpivot', 'update', 'updatetext',
    'use', 'user', 'values', 'varying', 'view', 'waitfor', 'when',
    'where', 'while', 'with', 'within', 'writetext'
]);

export const SqlConstants = {
    sqlTypeRegex: SQL_TYPE_REGEX,
    sqlTypes: SQL_TYPES,
    sqlTypeMap: SQL_TYPE_MAP,
    sqlAliasReservedWords: SQL_ALIAS_RESERVED_WORDS
};

