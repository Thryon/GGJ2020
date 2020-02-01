﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConditionList = GGJ.ConditionList;

[System.Serializable]
public class EventChoice
{
    public string choiceDescription;
    public string recapAfterChoice;
    public List<string> possibleChainedEvents;
    public float probaChainedEvent = 0.5f;
    public bool alterPrefab;
    public List<ConditionMalabarGroup> conditionsToEnableChoice;
    public ChoiceCost costs;
    public ChoiceRewards rewards;
    
    public bool IsChoiceEnabled()
    {
        return conditionsToEnableChoice.CheckAll();
    }

    /// <summary>
    /// Retourne l'id de l'event suivant, ou vide si on doit se déplacer ensuite
    /// </summary>
    /// <returns></returns>
    public string ChoiceConsequences()
    {
        if (PlayerData.CheckIfGameIsOver())
        {
            // Trigger final event
            return "";
        }
        else
        {
            if (possibleChainedEvents.Count > 0)
            {
                if (Random.Range(0, 1.0f) < probaChainedEvent)
                {
                    // Trigger random chained event
                    return possibleChainedEvents[Random.Range(0, possibleChainedEvents.Count)];
                }
                else
                {
                    // Move to next event
                    return "";
                }
            }
            else
            {
                // Move to next event
                return "";
            }
        }
    }


    [System.Serializable]
    public class ChoiceRewards
    {
        public int timeGain;
        public int charactersGain;
        public int toolsGain;

        public List<string> namedCharacters;
        public List<string> namedTools;

        public void Gain()
        {
            PlayerData.timeLeft += timeGain;

            for (int i = 0; i < charactersGain; i++)
                PlayerData.characters.Add(new GameplayRessource(
                    WordManager.Instance.noms.wordList[Random.Range(0, WordManager.Instance.noms.wordList.Count)])
                );

            for (int i = 0; i < toolsGain; i++)
                PlayerData.tools.Add(new GameplayRessource());

            foreach (var namedChara in namedCharacters)
                PlayerData.characters.Add(new GameplayRessource(namedChara));

            foreach (var namedTool in namedTools)
                PlayerData.tools.Add(new GameplayRessource(namedTool));
        }
    }

    [System.Serializable]
    public class ChoiceCost
    {
        public int timeCost;
        public int charactersCost;
        public int toolsCost;

        public bool lethalForRessources;
        public bool setToDamaged;
        public List<string> namedCharacters;
        public List<string> namedTools;

        // Variables feedback
        [Header("Variables feedback")]
        private bool showTimeCostOnRecap = true;
        bool showCharacterCostOnRecap = true;
        bool showToolsCostOnRecap = true;
        bool showSpecificCharactersRecap = true;
        bool showSpecificToolsRecap = true;

        string overrideRecapCharacter = "";
        string overrideRecapTool = "";
        string overrideRecapCharacterDeath = "";
        string overrideRecapToolBroken = "";


        public string ResolveCosts()
        {
            string feedback = "";
            PlayerData.timeLeft -= timeCost;
            if (showTimeCostOnRecap)
                feedback += "Cette action vous a pris " + timeCost + " unités de temps. ";

            CheckRessources(namedCharacters, true, feedback);
            CheckRessources(namedTools, false, feedback);

            NotNamedCosts(PlayerData.characters, true, feedback);
            NotNamedCosts(PlayerData.tools, false, feedback);

            return feedback;
        }

        void CheckRessources(List<string> _ids, bool _isCharacter, string _feedback)
        {
            foreach (var id in _ids)
            {
                if (_isCharacter)
                {
                    if (showSpecificCharactersRecap)
                        _feedback = PlayerData.DamageRessource(id, _isCharacter, lethalForRessources, setToDamaged, _feedback, overrideRecapCharacter, overrideRecapCharacterDeath);
                    else
                        PlayerData.DamageRessource(id, _isCharacter, lethalForRessources, setToDamaged, null, "", "");
                }
                else
                {
                    if (showSpecificToolsRecap)
                        _feedback = PlayerData.DamageRessource(id, _isCharacter, lethalForRessources, setToDamaged, _feedback, overrideRecapTool, overrideRecapToolBroken);
                    else
                        PlayerData.DamageRessource(id, _isCharacter, lethalForRessources, setToDamaged, null, "", "");
                }
            }
        }

        void NotNamedCosts(List<GameplayRessource> _playerData, bool _isCharacter, string _feedback)
        {
            List<GameplayRessource> gameplayRessourcesNotNamed = _playerData.FindAll(x => string.IsNullOrEmpty(x.ressourceName));
            int diff = gameplayRessourcesNotNamed.Count - ((_isCharacter) ? charactersCost : toolsCost);
            for (int i = 0; i < ((_isCharacter) ? charactersCost : toolsCost) && i < gameplayRessourcesNotNamed.Count; i++)
            {
                _playerData.Remove(gameplayRessourcesNotNamed[i]);
            }

            if (_isCharacter && showCharacterCostOnRecap)
                _feedback += "Vous avez perdu " + ((diff > 0) ? charactersCost : charactersCost + diff) + " unités. ";

            if (!_isCharacter && showToolsCostOnRecap)
                _feedback += "Vous avez perdu " + ((diff > 0) ? toolsCost : toolsCost + diff) + " outils. ";

            if (diff < 0)
            {
                PlayerData.ShuffleRessources();
                List<GameplayRessource> gameplayRessourcesNamed = _playerData.FindAll(x => !string.IsNullOrEmpty(x.ressourceName));
                for (int i = 0; i < diff && i < gameplayRessourcesNamed.Count; i++)
                {
                    if (_isCharacter)
                    {
                        if (showSpecificCharactersRecap)
                            _feedback = PlayerData.DamageRessource(gameplayRessourcesNamed[i].ressourceName, _isCharacter, lethalForRessources, setToDamaged, _feedback, overrideRecapCharacter, overrideRecapCharacterDeath);
                        else
                            PlayerData.DamageRessource(gameplayRessourcesNamed[i].ressourceName, _isCharacter, lethalForRessources, setToDamaged, null, "", "");
                    }
                    else
                    {
                        if (showSpecificToolsRecap)
                            _feedback = PlayerData.DamageRessource(gameplayRessourcesNamed[i].ressourceName, _isCharacter, lethalForRessources, setToDamaged, _feedback, overrideRecapTool, overrideRecapToolBroken);
                        else
                            PlayerData.DamageRessource(gameplayRessourcesNamed[i].ressourceName, _isCharacter, lethalForRessources, setToDamaged, null, "", "");
                    }
                }
            }
        }
    }
}
