﻿using Dooggy.Factory;
using Dooggy.Factory.Console;
using Dooggy.Lib.Generic;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dooggy.Factory.Console
{

    public enum eTipoTestCommand : int
    {

        fail = -1,

        note = 0,
        var = 1,

        raw = 10,

        view = 20,
        item = 21,

        save = 50,

    }

    public class TestBuilder
    {

        private TestCode Code;

        private TestSintaxe Sintaxe;

        public TestConsole Console { get => Code.Console; }
        public TestScript Script { get => Code.Script; }
        private TestBlocks Blocks { get => Code.Blocks; }
        public TestResult Result { get => Code.Result; }
        public TestTrace Trace { get => Console.Trace; }

        public TestBuilder(TestCode prmCode)
        {

            Code = prmCode;

            Sintaxe = new TestSintaxe(this);

        }

        public void Compile(string prmCode)
        {
            Result.SetCodeZero(); 
            
            Merge(prmCode);

            if (Blocks.IsNeedSaveData)
                Merge(prmCode: Console.Config.Format.saveKeywordDefault);
        }

        private void Merge(string prmCode)
        {

            Result.SetCodeZero(prmCode);

            foreach (string linha in new xLinhas(prmCode))
            {

                if (Sintaxe.IsNewLine(linha))
                {

                    if (Sintaxe.IsNewTag())
                    {


                    }
                    else if (Sintaxe.IsNewCommand())
                        Blocks.AddCommand(Sintaxe);
                    else
                        Blocks.AddParameter(Sintaxe);

                }

                Blocks.AddLine(linha);
            }

        }
    }
    public class TestSintaxe : TestSintaxeCommand
    {
        public TestConsole Console => Builder.Console;
        
        private string linha;

        private string keyword_note = ">>";

        private string keyword_start = ">";
        private string keyword_finish = ":";

        private string option_start = "[";
        private string option_finish = "]";

        private string tag_start = "<";
        private string tag_finish = ">";

        private bool TemKeyword() => (keyword != "");
        private bool TemOption() => (options != "");
        public TestSintaxe(TestBuilder prmBuilder)
        {

            Builder = prmBuilder;

        }

        public bool IsNewLine(string prmLinha)
        {

            linha = prmLinha.Trim();

            Cleanup();

            return (linha != "");

        }

        public bool IsNewTag()
        {

            return (false);


        }
        public bool IsNewCommand()
        {

            if (IsPrefixoNote())
                GetTargetNote();

            else if (IsNewKeyword())
                GetTargetCommand();

            if (TemKeyword())
                { Setup(); return (true); }

            return (false);

        }

        public bool IsNewParametro() => Argumento.IsNewParametro(linha);

        private bool IsPrefixoNote() => (Prefixo.IsPrefixo(linha, prmPrefixo: keyword_note));

        private bool IsPrefixoTag() => (Prefixo.IsPrefixo(linha, prmPrefixo: tag_start, prmDelimitador: tag_finish));

        private bool IsNewKeyword()
        {

            keyword = GetKeywordCommand();

            options = GetOptionsCommand();

            key = GetBaseKeyCommand();

            return (TemKeyword());

        }

        private bool  GetTagBlock()
        {

            tag = Prefixo.GetPrefixo(linha, prmPrefixo: tag_start, prmDelimitador: tag_finish).ToUpper();

            return (tag != "");

        }
        
        private void GetTargetNote()
        {

            key = keyword_note;

            keyword = keyword_note;

            target = Prefixo.GetPrefixoRemove(linha, prmPrefixo: keyword_note, prmTRIM: true);

        }

        private void GetTargetCommand()
        {

            target = Prefixo.GetPrefixoRemove(linha, prmPrefixo: keyword_start, prmDelimitador: keyword_finish, prmTRIM: true);

        }

        private string GetKeywordCommand()
        {

            return Prefixo.GetPrefixo(linha, prmPrefixo: keyword_start, prmDelimitador: keyword_finish).ToLower();

        }
        private string GetOptionsCommand()
        {

            return Bloco.GetBloco(keyword, prmDelimitadorInicial: option_start, prmDelimitadorFinal: option_finish);

        }

        private string GetBaseKeyCommand()
        {

            return Bloco.GetBlocoAntes(keyword, prmDelimitador: option_start, prmTRIM: true);

        }

        public TestSintaxe IClone() => (TestSintaxe)this.MemberwiseClone();

    }

    public class TestSintaxeCommand
    {

        public TestBuilder Builder;

        public TestTrace Trace { get => Builder.Trace; }
        public TestScript Script { get => Builder.Script; }
        

        public string key;

        public string tag;

        public string keyword;

        public string target;

        public string options;

        private string args;

        public eTipoTestCommand tipo;

        public string comando { get => GetComando(); }

        public TestSintaxeArgumento Argumento;

        public TestSintaxeCommand()
        {

            Argumento = new TestSintaxeArgumento();

        }

        protected void Setup()
        {
            switch (key)
            {

                case ">>":
                case "note":
                    tipo = eTipoTestCommand.note;
                    break;

                case "raw":
                case "data":
                    tipo = eTipoTestCommand.raw;
                    args = "header;null;*";
                    break;

                case "var":
                case "variavel":
                    tipo = eTipoTestCommand.var;
                    args = "sql";
                    break;

                case "view":
                case "dataview":
                    tipo = eTipoTestCommand.view;
                    args = "descricao;tabelas;campos;relacoes,mask;saida";
                    break;

                case "item":
                case "datafluxo":
                    tipo = eTipoTestCommand.item;
                    args = "sql;filtro;ordem";
                    break;

                case "save":
                case "datasave":
                    tipo = eTipoTestCommand.save;
                    args = "";//args = "encode;extensao";
                    break;

                //case "txt":
                //case "savetxt":
                //    tipo = eTipoTestCommand.savetxt;
                //    args = "";//args = "encode;extensao";
                //    break;

                //case "csv":
                //case "savecsv":
                //    tipo = eTipoTestCommand.savecsv;
                //    args = "";//args = "encode;extensao";
                //    break;

                //case "json":
                //case "savejson":
                //    tipo = eTipoTestCommand.savejson;
                //    args = "";//args = "encode;extensao";
                //    break;

                default:
                    tipo = eTipoTestCommand.fail;
                    Trace.LogConsole.FailFindKeyWord(keyword);

                    return;
            }

            Argumento.Setup(args);

            Trace.LogConsole.WriteKeyWord(keyword, target);

        }

        private string GetComando()
        {

            switch (tipo)
            {

                case eTipoTestCommand.note:
                    return "note";

                case eTipoTestCommand.raw:
                    return "data";

                case eTipoTestCommand.var:
                    return "variavel";

                case eTipoTestCommand.view:
                    return "dataview";

                case eTipoTestCommand.item:
                    return "datafluxo";

                case eTipoTestCommand.save:
                    return "save";

                //case eTipoTestCommand.savetxt:
                //    return "savetxt";

                //case eTipoTestCommand.savecsv:
                //    return "savecsv";

                //case eTipoTestCommand.savejson:
                //    return "savejson";

                case eTipoTestCommand.fail:
                    return "fail";
            }

            return("null");

        }
        protected void Cleanup()
        {

            key = "";
            keyword = "";
            target = "";
            options = "";

        }

    }

    public class TestSintaxeArgumento
    {

        public string key;

        public string parametro;

        public string linha;

        private xLista lista;

        private string arg_start = "-";
        private string arg_finish = ":";

        private string arg_default => arg_start + arg_finish;

        private bool IsArg() => (key != "");
        public bool IsOk() => lista.IsContem(key);

        public void Setup(string prmArgs)
        {

            lista = new xLista(prmArgs);

        }
        public bool IsNewParametro(string prmLinha)
        {

            linha = prmLinha.Trim();

            if (GetArgCommand())
            {

                GetArgDescription();

                return true;

            }

            parametro = linha;

            return false;

        }

        private bool GetArgCommand()
        {

            parametro = "";

            if (linha.StartsWith(arg_default))
                key = "null";
            else
                key = Bloco.GetBloco(linha, prmDelimitadorInicial: arg_start, prmDelimitadorFinal: arg_finish);

            return (IsArg());

        }

        private void GetArgDescription()
        {

            parametro = xString.GetLast(linha, prmDelimitador: ":").Trim();

        }

    }

}