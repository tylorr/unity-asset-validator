﻿/*
unity-asset-validator Copyright (C) 2017  Jeff Campbell

unity-asset-validator is licensed under a
Creative Commons Attribution-NonCommercial 4.0 International License.

You should have received a copy of the license along with this
work. If not, see <http://creativecommons.org/licenses/by-nc/4.0/>.
*/
namespace JCMG.AssetValidator.Editor.Validators.Output
{
    public partial class VLog
    {
        public VLogType vLogType;
        public VLogSource source;
        public string validatorName;
        public string message;
        public string scenePath;
        public string objectPath;
    }
}