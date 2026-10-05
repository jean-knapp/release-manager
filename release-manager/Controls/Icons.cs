namespace ReleaseManager.Controls
{
    /// <summary>
    /// SVG glyphs from the FTP Client handoff, on its 24 and 16 unit grids. Every glyph uses
    /// <c>currentColor</c>, so the controls tint them to the token they sit in.
    /// </summary>
    public static class Icons
    {
        private const string Open = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\">";
        private const string Open16 = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 16 16\">";
        private const string Open12 = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 12 12\">";
        private const string Close = "</svg>";

        // ------------------------------------------------------------------ servers and files

        public const string Server = Open + "<path fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" d=\"M4 6h16v5H4zM4 15h16v5H4z\"/><circle fill=\"currentColor\" cx=\"7.5\" cy=\"8.5\" r=\"1.2\"/><circle fill=\"currentColor\" cx=\"7.5\" cy=\"17.5\" r=\"1.2\"/>" + Close;
        /// <summary>The server outline without its status lights, used inside the breadcrumb field.</summary>
        public const string ServerOutline = Open + "<path fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" d=\"M4 6h16v5H4zM4 15h16v5H4z\"/>" + Close;
        public const string Folder = Open + "<path fill=\"currentColor\" d=\"M3 5h6l2 2h10v12H3z\"/>" + Close;
        public const string FolderOpen = Open + "<path fill=\"currentColor\" d=\"M3 5h6l2 2h9v3H7.5L5 18H3z\"/><path fill=\"currentColor\" d=\"M7.8 11H22l-3 8H5z\"/>" + Close;
        public const string File = Open + "<path fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" d=\"M6 3h8l4 4v14H6z\"/>" + Close;
        /// <summary>A file with its folded corner, for the properties dialog tile.</summary>
        public const string FileDetailed = Open + "<path fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" d=\"M6 3h8l4 4v14H6z\"/><path fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" d=\"M13 3v5h5\"/>" + Close;
        public const string FolderLink = Open + "<path fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" d=\"M4 6h5l2 2h9v11H4z\"/><path fill=\"currentColor\" d=\"M8.5 17l4.3-4.3H10v-1.7h5.5v5.5h-1.7v-2.8L9.7 18z\"/>" + Close;
        public const string Symlink =Open + "<path fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" d=\"M6 3h8l4 4v14H6z\"/><path fill=\"currentColor\" d=\"M9 16l4.3-4.3H10.5V10H16v5.5h-1.7v-2.8L10 17z\"/>" + Close;

        // ------------------------------------------------------------------ transfers

        public const string Upload = Open + "<path fill=\"currentColor\" d=\"M12 3l5.9 5.9-1.4 1.4L13 6.8V16h-2V6.8L7.5 10.3 6.1 8.9z\"/><path fill=\"currentColor\" d=\"M4 18h16v2H4z\"/>" + Close;
        public const string Download = Open + "<path fill=\"currentColor\" d=\"M11 3h2v9.2l3.5-3.5 1.4 1.4L12 16 6.1 10.1l1.4-1.4L11 12.2z\"/><path fill=\"currentColor\" d=\"M4 18h16v2H4z\"/>" + Close;
        /// <summary>The upload arrow into an open tray, drawn in the drop target.</summary>
        public const string UploadTray = Open + "<path fill=\"currentColor\" d=\"M12 3l5.9 5.9-1.4 1.4L13 6.8V15h-2V6.8L7.5 10.3 6.1 8.9z\"/><path fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" d=\"M4 15v5h16v-5\"/>" + Close;
        public const string Pause = Open + "<path fill=\"currentColor\" d=\"M7 5h3v14H7zM14 5h3v14h-3z\"/>" + Close;
        public const string Resume = Open + "<path fill=\"currentColor\" d=\"M8 5l11 7-11 7z\"/>" + Close;

        // ------------------------------------------------------------------ commands

        public const string NewFolder = Open + "<path fill=\"currentColor\" d=\"M3 5h6l2 2h10v12H3z\"/><path fill=\"#000\" fill-opacity=\"0.55\" d=\"M12 11h2v2h2v2h-2v2h-2v-2h-2v-2h2z\"/>" + Close;
        public const string Rename = Open + "<path fill=\"currentColor\" d=\"M3 17.3V21h3.7L17.8 9.9l-3.7-3.7zM20.7 7a1 1 0 0 0 0-1.4l-2.3-2.3a1 1 0 0 0-1.4 0l-1.8 1.8 3.7 3.7z\"/>" + Close;
        public const string Delete = Open + "<path fill=\"currentColor\" d=\"M6 7h12l-1 13H7zM9 3h6v2H9z\"/>" + Close;
        public const string Permissions = Open + "<path fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.5\" d=\"M7.5 10V7.5a4.5 4.5 0 0 1 9 0V10\"/><rect fill=\"currentColor\" x=\"5\" y=\"10\" width=\"14\" height=\"10\" rx=\"2\"/>" + Close;
        public const string Refresh = Open + "<path fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" d=\"M19 12c0 3.9-3.1 7-7 7s-7-3.1-7-7 3.1-7 7-7c2.4 0 4.5 1.2 5.8 3\"/><path fill=\"currentColor\" d=\"M20 3v6h-6z\"/>" + Close;
        public const string Search = Open16 + "<circle cx=\"6.5\" cy=\"6.5\" r=\"4.6\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.6\"/><path stroke=\"currentColor\" stroke-width=\"1.6\" d=\"M10 10l4 4\"/>" + Close;
        public const string List = Open + "<path fill=\"currentColor\" d=\"M4 6h16v2H4zM4 11h16v2H4zM4 16h16v2H4z\"/>" + Close;
        public const string Grid = Open + "<path fill=\"currentColor\" d=\"M4 4h7v7H4zM13 4h7v7h-7zM4 13h7v7H4zM13 13h7v7h-7z\"/>" + Close;
        public const string Terminal = Open + "<path fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" d=\"M4 5h16v14H4z\"/><path fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" d=\"M7 9l3 3-3 3M12 15h5\"/>" + Close;
        public const string Settings = Open + "<circle fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" cx=\"12\" cy=\"12\" r=\"3\"/><path fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" d=\"M12 3v3M12 18v3M3 12h3M18 12h3\"/>" + Close;
        public const string Copy = Open + "<path fill=\"currentColor\" d=\"M8 8h12v12H8z\"/><path fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" d=\"M16 8V4H4v12h4\"/>" + Close;
        public const string Bookmark = Open + "<path fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" d=\"M12 4v10\"/><path fill=\"currentColor\" d=\"M12 20l-4-5h8z\"/><path fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" d=\"M4 8h3M17 8h3\"/>" + Close;

        // ------------------------------------------------------------------ navigation

        public const string Back = Open + "<path fill=\"currentColor\" d=\"M13 5l-7 7 7 7v-4h6v-6h-6z\"/>" + Close;
        public const string Forward = Open + "<path fill=\"currentColor\" d=\"M11 5l7 7-7 7v-4H5V9h6z\"/>" + Close;
        public const string Up = Open + "<path fill=\"currentColor\" d=\"M11 4h2v10.2l3.5-3.5 1.4 1.4L12 18l-5.9-5.9 1.4-1.4L11 14.2z\" transform=\"rotate(180 12 11)\"/>" + Close;
        public const string ChevronDown = Open12 + "<path fill=\"currentColor\" d=\"M1.5 4L6 8.5 10.5 4l-.9-.9L6 6.7 2.4 3.1z\"/>" + Close;
        public const string ChevronLeft = Open12 + "<path fill=\"currentColor\" d=\"M8 1.5L3.5 6 8 10.5l.9-.9L5.3 6 8.9 2.4z\"/>" + Close;
        public const string ChevronRight = Open12 + "<path fill=\"currentColor\" d=\"M4 1.5L8.5 6 4 10.5l-.9-.9L6.7 6 3.1 2.4z\"/>" + Close;
        public const string ChevronUp = Open12 + "<path fill=\"currentColor\" d=\"M1.5 8L6 3.5 10.5 8l-.9.9L6 5.3 2.4 8.9z\"/>" + Close;

        // ------------------------------------------------------------------ pairing and git

        public const string Link = Open + "<path fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" d=\"M10 14a4 4 0 0 0 6 .5l2.5-2.5a4 4 0 0 0-5.7-5.7L11.5 7.7\"/><path fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" d=\"M14 10a4 4 0 0 0-6-.5L5.5 12a4 4 0 0 0 5.7 5.7l1.3-1.3\"/>" + Close;
        public const string Branch = Open + "<circle fill=\"currentColor\" cx=\"6\" cy=\"5\" r=\"2.6\"/><circle fill=\"currentColor\" cx=\"6\" cy=\"19\" r=\"2.6\"/><circle fill=\"currentColor\" cx=\"18\" cy=\"8\" r=\"2.6\"/><path fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" d=\"M6 7.5v9M18 10.5c0 5-12 2-12 6\"/>" + Close;
        public const string Github = Open + "<path fill=\"currentColor\" d=\"M12 2a10 10 0 0 0-3.2 19.5c.5.1.7-.2.7-.5v-1.7c-2.8.6-3.4-1.2-3.4-1.2-.4-1.2-1.1-1.5-1.1-1.5-.9-.6.1-.6.1-.6 1 .1 1.5 1 1.5 1 .9 1.6 2.4 1.1 3 .9.1-.7.4-1.1.6-1.4-2.2-.2-4.6-1.1-4.6-4.9 0-1.1.4-2 1-2.7-.1-.3-.4-1.3.1-2.7 0 0 .8-.3 2.8 1a9.5 9.5 0 0 1 5 0c1.9-1.3 2.8-1 2.8-1 .5 1.4.2 2.4.1 2.7.6.7 1 1.6 1 2.7 0 3.8-2.3 4.7-4.6 4.9.4.3.7.9.7 1.9v2.8c0 .3.2.6.7.5A10 10 0 0 0 12 2z\"/>" + Close;

        // ------------------------------------------------------------------ state and security

        public const string Padlock = Open16 + "<path fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.5\" d=\"M4.75 7V5a3.25 3.25 0 0 1 6.5 0v2\"/><rect x=\"3\" y=\"7\" width=\"10\" height=\"7\" rx=\"1.5\" fill=\"currentColor\"/>" + Close;
        public const string Key = Open16 + "<circle cx=\"5.5\" cy=\"8\" r=\"3\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.6\"/><path stroke=\"currentColor\" stroke-width=\"1.6\" d=\"M8.5 8H14M12 8v3\"/>" + Close;
        public const string Warning = Open + "<path fill=\"currentColor\" d=\"M12 2.4l10.4 18H1.6z\"/><path fill=\"#000\" fill-opacity=\"0.55\" d=\"M11 9h2v6h-2zM11 16.2h2v2.2h-2z\"/>" + Close;
        /// <summary>The horizontal bar on the queue's collapse button.</summary>
        public const string Collapse = Open16 + "<path fill=\"currentColor\" d=\"M1 7h14v2H1z\"/>" + Close;

        // ------------------------------------------------------------------ small controls

        public const string Plus = Open + "<path fill=\"currentColor\" d=\"M11 5h2v6h6v2h-6v6h-2v-6H5v-2h6z\"/>" + Close;
        public const string Cross = Open + "<path fill=\"currentColor\" d=\"M6.4 5L19 17.6 17.6 19 5 6.4z\"/><path fill=\"currentColor\" d=\"M17.6 5L5 17.6 6.4 19 19 6.4z\"/>" + Close;
        public const string Check = Open + "<path fill=\"currentColor\" d=\"M9 16.2l-3.5-3.5-1.4 1.4L9 19 20 8l-1.4-1.4z\"/>" + Close;

        // ------------------------------------------------------------------ databases

        /// <summary>The stacked cylinder of a database.</summary>
        public const string Database = Open + "<ellipse fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" cx=\"12\" cy=\"6\" rx=\"7\" ry=\"2.6\"/><path fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" d=\"M5 6v12c0 1.4 3.1 2.6 7 2.6s7-1.2 7-2.6V6M5 12c0 1.4 3.1 2.6 7 2.6s7-1.2 7-2.6\"/>" + Close;
        public const string Table = Open + "<path fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" d=\"M4 5h16v14H4zM4 10h16M4 14.5h16M10 10v9\"/>" + Close;
        /// <summary>A table seen through a lens: views are saved queries, not stored rows.</summary>
        public const string View = Open + "<path fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" d=\"M2.5 12S6 6 12 6s9.5 6 9.5 6-3.5 6-9.5 6-9.5-6-9.5-6z\"/><circle fill=\"currentColor\" cx=\"12\" cy=\"12\" r=\"2.6\"/>" + Close;
        public const string Run = Open + "<path fill=\"currentColor\" d=\"M8 5l11 7-11 7z\"/>" + Close;
        public const string Stop = Open + "<path fill=\"currentColor\" d=\"M6 6h12v12H6z\"/>" + Close;
        /// <summary>A column list, for the Structure tab.</summary>
        public const string Columns = Open + "<path fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" d=\"M4 5h16v14H4zM9.5 5v14M14.5 5v14\"/>" + Close;
        /// <summary>A code page, for the SQL tab and new queries.</summary>
        public const string Query = Open + "<path fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" d=\"M5 3h10l4 4v14H5z\"/><path fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" d=\"M9.5 11l-2 2 2 2M14.5 11l2 2-2 2\"/>" + Close;
        public const string Export = Open + "<path fill=\"currentColor\" d=\"M12 3l5.9 5.9-1.4 1.4L13 6.8V15h-2V6.8L7.5 10.3 6.1 8.9z\"/><path fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" d=\"M5 14v6h14v-6\"/>" + Close;
        public const string Filter = Open + "<path fill=\"currentColor\" d=\"M3 5h18l-7 8v6l-4-2v-4z\"/>" + Close;

        // ------------------------------------------------------------------ release steps

        /// <summary>A hammer, for the build step.</summary>
        public const string Hammer = Open + "<path fill=\"currentColor\" d=\"M13.5 3l5.5 5.5-2.1 2.1-1.6-1.6-1.9 1.9 1.3 1.3-1.8 1.8-7 7-2.5-2.5 7-7-1.3-1.3 1.9-1.9-1.6-1.6z\"/>" + Close;
        /// <summary>A shield, for obfuscation.</summary>
        public const string Shield = Open + "<path fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" d=\"M12 3l7 2.5v5.5c0 4.5-3 8-7 9.5-4-1.5-7-5-7-9.5V5.5z\"/><path fill=\"currentColor\" d=\"M11 13.2l-2-2-1.3 1.4L11 16l5-5-1.4-1.4z\"/>" + Close;
        /// <summary>A shipping box, for packaging.</summary>
        public const string Box = Open + "<path fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" d=\"M12 3l8 4.3v9.4L12 21l-8-4.3V7.3z\"/><path fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" d=\"M4 7.3l8 4.3 8-4.3M12 11.6V21\"/>" + Close;

        // ------------------------------------------------------------------ shell

        /// <summary>Two slider tracks with their knobs at different places, for Settings (from mlp-rv's NavPane).</summary>
        public const string Sliders = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 18 18\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.6\" stroke-linecap=\"round\" stroke-linejoin=\"round\">"
            + "<path d=\"M2.6 5.6h6.2M12.8 5.6h2.6M2.6 12.4h2.6M9.2 12.4h6.2\"/><path d=\"M10.8 3.6v4M7.2 10.4v4\"/>" + Close;

        // ------------------------------------------------------------------ logo

        private const string LogoOpen = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 48 48\">";
        private const string LogoArrow = "M15.5 37.2v-9.4l-3 1.5 4.6-7.6 4.6 12.2-3-1.5v6.3z";

        /// <summary>The app logo: a package with a release arrow on its front-left face, light theme.</summary>
        public const string LogoLight = LogoOpen + LogoFaces + "<path fill=\"#FFFFFF\" d=\"" + LogoArrow + "\"/>" + Close;

        /// <summary>The app logo for the dark theme.</summary>
        public const string LogoDark = LogoOpen + LogoFacesDark + "<path fill=\"#003E7A\" d=\"" + LogoArrow + "\"/>" + Close;

        /// <summary>The logo at 16 px, where the arrow would only blur: the box alone.</summary>
        public const string Logo16Light = LogoOpen + LogoFaces + Close;
        public const string Logo16Dark = LogoOpen + LogoFacesDark + Close;

        private const string LogoFaces =
            "<path fill=\"#4CC2FF\" d=\"M24 6l17 8.5-17 8.5-17-8.5z\"/>" +
            "<path fill=\"#005FB8\" d=\"M7 14.5l17 8.5v19L7 33.5z\"/>" +
            "<path fill=\"#003E7A\" d=\"M41 14.5L24 23v19l17-8.5z\"/>";

        private const string LogoFacesDark =
            "<path fill=\"#99DDFF\" d=\"M24 6l17 8.5-17 8.5-17-8.5z\"/>" +
            "<path fill=\"#4CC2FF\" d=\"M7 14.5l17 8.5v19L7 33.5z\"/>" +
            "<path fill=\"#1C8ACB\" d=\"M41 14.5L24 23v19l17-8.5z\"/>";
    }
}
