﻿/*
unity-asset-validator Copyright (C) 2017  Jeff Campbell

unity-asset-validator is licensed under a
Creative Commons Attribution-NonCommercial 4.0 International License.

You should have received a copy of the license along with this
work. If not, see <http://creativecommons.org/licenses/by-nc/4.0/>.
*/
using JCMG.AssetValidator.Editor.Meta;
using JCMG.AssetValidator.Editor.Validators.Output;
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace JCMG.AssetValidator.Editor.Validators
{
    public abstract class BaseInstanceValidatorManager : BaseValidatorManager
    {
        protected List<Object> _objectsToValidate;
        protected Dictionary<Type, List<AbstractInstanceValidator>> _typeToValidatorsLookup;
        protected readonly ClassTypeCache _cache;
        protected readonly InstanceValidatorCache _instanceValidatorCache;

        public BaseInstanceValidatorManager(ClassTypeCache cache, AssetValidatorLogger logger) 
            : base(logger)
        {
            _cache = cache;
            _instanceValidatorCache = new InstanceValidatorCache();

            InitValidatorLookup();
        }

        /// <summary>
        /// Build up a lookup table of type to validators to be able to apply that set of validators to 
        /// an instance of that type later on.
        /// </summary>
        protected virtual void InitValidatorLookup()
        {
            _objectsToValidate = new List<Object>();
            _typeToValidatorsLookup = new Dictionary<Type, List<AbstractInstanceValidator>>();

            for (var i = 0; i < _cache.Count; i++)
            {
                for (var j = 0; j < _instanceValidatorCache.Count; j++)
                {
                    if (!_instanceValidatorCache[j].AppliesTo(_cache[i])) continue;

                    if (!_typeToValidatorsLookup.ContainsKey(_cache[i]))
                        _typeToValidatorsLookup.Add(_cache[i], new List<AbstractInstanceValidator>());

                    _typeToValidatorsLookup[_cache[i]].Add(_instanceValidatorCache[j]);
                }
            }

            if (_logger == null) return;

            for (var i = 0; i < _instanceValidatorCache.Count; i++)
                _instanceValidatorCache[i].OnLogEvent += OnLogEvent;
        }

        public override void ValidateAll()
        {
            for (var i = 0; i < _objectsToValidate.Count; i++)
            {
                var evalType = _objectsToValidate[i].GetType();
                if (!_typeToValidatorsLookup.ContainsKey(evalType)) continue;

                var validators = _typeToValidatorsLookup[evalType];
                for (var j = 0; j < validators.Count; j++)
                    validators[j].Validate(_objectsToValidate[i]);
            }

            // Mark continuous progress as the total count to ensure the ActiveSceneValidator shows as complete.
            _continousProgress = _objectsToValidate.Count;
        }

        public override bool ContinueValidation()
        {
            if (_continousProgress >= _objectsToValidate.Count) return false;

            var nextStep = _continousProgress + _continuousObjectsPerStep >= _objectsToValidate.Count
                ? _objectsToValidate.Count
                : _continousProgress + _continuousObjectsPerStep;

            for (; _continousProgress < nextStep; _continousProgress++)
            {
                var evalType = _objectsToValidate[_continousProgress].GetType();
                if (!_typeToValidatorsLookup.ContainsKey(evalType)) continue;

                var validators = _typeToValidatorsLookup[evalType];
                for (var j = 0; j < validators.Count; j++)
                    validators[j].Validate(_objectsToValidate[_continousProgress]);
            }

            return _continousProgress < _objectsToValidate.Count;
        }

        public override bool IsComplete()
        {
            return _continousProgress >= _objectsToValidate.Count;
        }

        public override float GetProgress()
        {
            return _continousProgress / (float)_objectsToValidate.Count;
        }

        /// <summary>
        /// Only used for unit tests, TODO Refactor to hide public method
        /// </summary>
        /// <returns></returns>
        public List<Object> GetObjectsToValidate()
        {
            return _objectsToValidate;
        }

        public override void Search()
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            if (_logger == null) return;

            for (var i = 0; i < _instanceValidatorCache.Count; i++)
                _instanceValidatorCache[i].OnLogEvent -= OnLogEvent;

            base.Dispose();
        }
    }
}
