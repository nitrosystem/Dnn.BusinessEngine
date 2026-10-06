export const activityBarItems = [
    {
        name: 'explorer',
        title: 'Explorer',
        icon: 'files',
        visible: true,
    },
    {
        name: 'builder',
        title: 'Builder',
        icon: 'symbol-color',
        visible: true,
    },
    {
        name: 'extensions',
        title: 'Extensions',
        icon: 'extensions',
        callback: 'onGotoExtensions',
        visible: true
    },
    {
        name: 'diagnostic-entries',
        title: 'Diagnostic Entries',
        icon: 'output',
        callback: 'onGotoDiagnosticEntries',
        visible: true
    }
];