﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using Dalutex.Models;
using System.Configuration;
using Dalutex.Models.DataModels;
using System.Data.Entity;
using System.Web.Helpers;
using System.IO;
using Dalutex.Models.Utils;
using System.Net.Mail;
using System.Drawing;
using Microsoft.Reporting.WebForms;
using System.Text;

namespace Dalutex.Controllers
{
    public class PedidoController : BaseController
    {
        #region Shared

        private ActionResult ValidarTipoPedido(object model)
        {
            if (base.Session_Carrinho != null && base.Session_Carrinho.IDTipoPedido != -1) //Pedido de algum tipo já iniciado
            {
                if (base.Session_Carrinho.Itens == null || base.Session_Carrinho.Itens.Count == 0)//Se já foi iniciado mas ainda não tem itens, limpar
                {
                    base.Session_Carrinho.IDTipoPedido = -1;
                    return null;
                }
                else
                {
                    if ((model is ItensParaReservaViewModel))//Se for pedido de reserva
                    {
                        if (base.Session_Carrinho.IDTipoPedido != (int)Enums.TiposPedido.RESERVA)
                        {
                            //Não permitir
                            return RedirectToAction("Message", new { message = "Iniciado pedido de venda. Esvazie o carrinho antes de incluir pedidos de reserva.", title = "Pedido já iniciado." });
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else//Outros tipos de pedidos
                    {
                        if (base.Session_Carrinho.IDTipoPedido == (int)Enums.TiposPedido.RESERVA)//Se uma reserva já tiver iniciada
                        {
                            //Não permitir
                            return RedirectToAction("Message", new { message = "Iniciado pedido de reserva. Esvazie o carrinho antes de incluir outros tipos de pedidos.", title = "Pedido já iniciado." });
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            else
            {
                return null;
            }
        }

        public ActionResult EmConstrucao()
        {
            return View();
        }

        public ActionResult ArtigosDisponiveis(
            string desenho,
            string variante,
            int idcolecao,
            string nmcolecao,
            int pagina,
            int idvariante,
            int pedidoreserva,
            int itempedidoreserva,
            int tipo)
        {
            ArtigosDisponiveisViewModel model = new ArtigosDisponiveisViewModel();
            model.Desenho = desenho;
            model.Variante = variante;
            model.IDColecao = idcolecao;
            model.NMColecao = nmcolecao;
            model.Pagina = pagina;
            model.Imagem = ConfigurationManager.AppSettings["PASTA_DESENHOS"] + desenho + "_" + variante + ".jpg";
            model.PedidoReserva = pedidoreserva;
            model.IDVariante = idvariante;
            model.ItemPedidoReserva = itempedidoreserva;
            model.Tipo = (Enums.ItemType)tipo;

            List<VW_CARACT_DESENHOS> lstQuery = null;

            using (var ctx = new TIDalutexContext())
            {
                var query =
                    from vw in ctx.VW_CARACT_DESENHOS
                    where vw.DESENHO == desenho
                    select vw;

                lstQuery = query.ToList();
            }

            VW_CARACT_DESENHOS objPrimeiroCarac = lstQuery.FirstOrDefault();
            int? iIDTecnologia;

            if (objPrimeiroCarac != null && objPrimeiroCarac.TECNOLOGIA != null)
            {
                model.TecnologiaAtual = objPrimeiroCarac.TECNOLOGIA.Replace(" ", "_");

                iIDTecnologia = objPrimeiroCarac.ID_TECNOLOGIA;

                List<int?> lstCaracteristicas = new List<int?>();

                foreach (VW_CARACT_DESENHOS item in lstQuery)
                {
                    lstCaracteristicas.Add(item.ID_CARAC_TEC);
                }

                using (var ctx = new TIDalutexContext())
                {
                    List<VW_TROCA_TEC> lstTrocas = ctx.VW_TROCA_TEC.Where(x => x.ID_TEC_ORIGINAL == iIDTecnologia).ToList();

                    List<int?> lstTecnologias = new List<int?>();
                    lstTecnologias.Add(iIDTecnologia);

                    foreach (VW_TROCA_TEC item in lstTrocas)
                    {
                        lstTecnologias.Add(item.ID_TEC_NOVA);
                    }

                    //-- oda 03/11/2015 -- alteração de validação da caracteristicas e tecnologias por artigos disponiveis --- 
                    List<PED_LINK_RESTRICAO_ARTIGO> lstArtigos = ctx.PED_LINK_RESTRICAO_ARTIGO.Where(x => lstCaracteristicas.Contains(x.ID_CARAC_TEC)).ToList();

                    List<string> lstArtigosStr = new List<string>();

                    foreach (PED_LINK_RESTRICAO_ARTIGO item in lstArtigos)
                    {
                        lstArtigosStr.Add(item.ARTIGO);
                    }
                    
                    var query =
                        from ar in ctx.VW_ARTIGOS_DISPONIVEIS
                        where (!lstArtigosStr.Contains(ar.ARTIGO))
                           && (lstTecnologias.Contains(ar.ID_TEC))
                        group ar by
                            new
                            {
                                ar.ARTIGO,
                                ar.TECNOLOGIA,
                            }
                            into grp
                            select new ArtigoTecnologia
                            {
                                Artigo = grp.Key.ARTIGO,
                                Tecnologia = grp.Key.TECNOLOGIA
                            };

                                                                                                                                                                                                        
                    //forma anterior. a estrutura da view mudou --------------------------------------------------
                    //var query =
                    //    from ar in ctx.VW_ARTIGOS_DISPONIVEIS
                    //    where (ar.ID_TECNOLOGIA.Equals(null) || ar.ID_TEC != iIDTecnologia)
                    //       && (ar.ID_CARAC_TEC.Equals(null) || !lstCaracteristicas.Contains(ar.ID_CARAC_TEC))
                    //       && (lstTecnologias.Contains(ar.ID_TEC))
                    //       && (lstTecnologias.Contains(ar.ID_TEC_ARTIGO))
                    //    group ar by
                    //        new
                    //        {
                    //            ar.ARTIGO,
                    //            ar.TECNOLOGIA,
                    //        }
                    //        into grp
                    //        select new ArtigoTecnologia
                    //        {
                    //            Artigo = grp.Key.ARTIGO,
                    //            Tecnologia = grp.Key.TECNOLOGIA
                    //        };

                    string _sql = query.ToString();//apenas para pegar o SQL que esta sendo passado

                    model.Artigos = query.OrderBy(x => x.Tecnologia).ThenBy(x => x.Artigo).ToList();
                }
            }

            return View(model);
        }

        public ActionResult Ampliacao(
            string desenho,
            string variante,
            string idcolecao,
            string nmcolecao,
            int pagina,
            string retornarpara,
            string codstudio,
            int tipo)
        {
            AmpliacaoViewModel model = new AmpliacaoViewModel()
            {
                Desenho = desenho,
                Variante = variante,
                IDColecao = idcolecao,
                NMColecao = nmcolecao,
                Pagina = pagina,
                RetornarPara = retornarpara,
                CodStudio = codstudio,
                //Tipo = (Enums.ItemType)tipo
                Tipo = tipo
            };

            if (tipo == (int)Enums.ItemType.Estampado || tipo == (int)Enums.ItemType.ValidacaoReserva)
            {
                model.Imagem = ConfigurationManager.AppSettings["PASTA_DESENHOS"].Replace("~", "") + desenho + "_" + variante + ".jpg";
            }
            else if (tipo == (int)Enums.ItemType.Reserva)
            {
                model.Imagem = ConfigurationManager.AppSettings["PASTA_RESERVAS"].Replace("~", "") + codstudio + ".jpg";
            }

            return View(model);
        }


        public ActionResult InserirNoCarrinho(
            string idcolecao
            , string nmcolecao
            , string pagina
            , string desenho
            , string variante
            , string artigo
            , string tecnologia
            , string tecnologiaoriginal
            , string cor
            , string modo
            , string rgb
            , int reduzido
            , string codstudio
            , string coddal
            , int tipo
            , int idstudio
            , int iditemstudio
            , int idvariante
            , int pedidoreserva
            , int itempedidoreserva)
        {
            InserirNoCarrinhoViewModel model = new InserirNoCarrinhoViewModel();

            model.Desenho = desenho;
            model.Variante = variante;
            model.Artigo = artigo;
            model.TecnologiaPorExtenso = tecnologia;
            model.TecnologiaOriginal = tecnologiaoriginal;
            model.IDColecao = idcolecao;
            model.NMColecao = nmcolecao;
            model.Cor = cor;
            model.RGB = rgb;
            model.Tipo = (Enums.ItemType)tipo;
            model.CodDal = coddal;
            model.CodStudio = codstudio;
            model.IDStudio = idstudio;
            model.IDItemStudio = iditemstudio;
            model.IDVariante = idvariante;
            model.PedidoReserva = pedidoreserva;
            model.ItemPedidoReserva = itempedidoreserva;
            model.Reduzido = reduzido;           

            if (base.Session_Carrinho != null)
                model.IDTipoPedido = base.Session_Carrinho.IDTipoPedido;

            if (pagina != null)
                model.Pagina = int.Parse(pagina);

            if (modo == "A")//Alterando item
            {
                model = base.Session_Carrinho.Itens.Where(x => x.Equals(model)).First();
            }

            model.Modo = modo;

            if (model.Tipo != Enums.ItemType.Reserva) //se for pedido diferente de reserva
            {
                using (var ctxTI = new TIDalutexContext())
                {
                    if (model.Tipo != Enums.ItemType.Liso) //se tipo for estampado
                    {
                        using (var ctx = new DalutexContext())
                        {
                            string _cor = "0000000";
                            if (model.Tipo == Enums.ItemType.ValidacaoReserva)//se for pedido de validação de reserva, procura pelo item exclusivo "E"
                            {
                                _cor = "E000000";
                            }

                            //procura o reduzido pela combinação de itens [Artigo, Tec, Desenho, Var]
                            VMASCARAPRODUTOACABADO objReduzido = ctx.VMASCARAPRODUTOACABADO.Where(
                                    x =>
                                        x.ARTIGO == model.Artigo
                                        && x.DESENHO == model.Desenho
                                        && x.VARIANTE == model.Variante
                                        && x.MAQUINA == model.Tecnologia
                                        && x.COR == _cor
                                    ).FirstOrDefault();

                            if (objReduzido != null && objReduzido.CODIGO_REDUZIDO > default(int))
                            {
                                model.Reduzido = objReduzido.CODIGO_REDUZIDO;
                            }
                            else
                                model.Reduzido = -2; //reduzido criado por JOB posteriormente
                        }
                    }                 
                    //int[] tiposPedidos = new int[] 
                    //{ 
                    //    (int)Enums.TiposPedido.AMOSTRA,
                    //    (int)Enums.TiposPedido.PILOTAGEM,
                    //    (int)Enums.TiposPedido.VENDA,
                    //    (int)Enums.TiposPedido.MOSTRUARIO,
                    //};
                    //model.TamanhoPadrao = ctxTIDalutex.REGRA_PADRAO.Where(x => tiposPedidos.Any(tipo => x.TIPOPEDIDO.Equals(tipo))).ToList();
                    //ctx.VW_FAROL.Where(x => x.ARTIGO == model.Artigo && x.DESENHO == model.Desenho && x.VARIANTE == model.Variante).FirstOrDefault();

                    CarregarTamanhosPadrao(model);

                    model.IDTamanhoPadrao = model.TamanhoPadrao[0].VALOR_PADRAO;
                   
                    //var query =
                    //    from app in ctxTI.ARTIGO_PESO_PADRAO
                    //    where
                    //        (
                    //            app.ATIVO == true
                    //            && app.ARTIGO == artigo
                    //            && app.TECNOLOGIA == tecnologia.Substring(0, 1)
                    //        )
                    //    select app;                
                    //ARTIGO_PESO_PADRAO objValorPadrao = query.FirstOrDefault();

                    //if (objValorPadrao != null)
                    //{
                    //    model.UnidadeMedida = objValorPadrao.UM;
                    //    model.ValorPadrao = objValorPadrao.VALOR;
                    //}                    
                    //else
                    //{
                    //    VMASCARAPRODUTOACABADO objValorPadraoView = null;

                    //    using (var ctx = new DalutexContext())
                    //    {
                    //        objValorPadraoView = ctx.VMASCARAPRODUTOACABADO.Where(x => x.ARTIGO == model.Artigo).FirstOrDefault();
                    //        if (objValorPadraoView != null)
                    //        {
                    //            model.UnidadeMedida = objValorPadraoView.UM;
                    //            if (model.UnidadeMedida.ToUpper() == "KG")
                    //            {
                    //                model.ValorPadrao = (decimal)Enums.ValorPadraoUnidade.Quilo;
                    //            }
                    //            else if (model.UnidadeMedida.ToUpper() == "MT")
                    //            {
                    //                model.ValorPadrao = (decimal)Enums.ValorPadraoUnidade.Metro;
                    //            }
                    //            else
                    //            {
                    //                ModelState.AddModelError("", "UNIDADE DE MEDIDA INVÁLIDA.");
                    //            }
                    //        }
                    //        else
                    //        {
                    //            ModelState.AddModelError("", "UNIDADE DE MEDIDA NÃO ENCONTRADA.");
                    //        }
                    //    }
                    //}
                }

                this.CarregarTiposPedidos(model);

                model.ObterTipoPedido = (base.Session_Carrinho == null || base.Session_Carrinho.IDTipoPedido < 0)? "S" : "N";
            }
            else
            {
                model.ObterTipoPedido = "N";
            }

            ViewBag.POGReduzido = model.Reduzido;

            return View(model);
        }

        private void CarregarTamanhosPadrao(InserirNoCarrinhoViewModel model)
        {
            using(var ctxTI = new TIDalutexContext())
            {
                if (model.Reduzido > -2) //se tem reduzido
                {
                    int _pi = 0;

                    model.UnidadeMedida = ctxTI.VW_PI_RED.Where(x => x.REDUZIDO == model.Reduzido).FirstOrDefault().UM;

                    // pega o PI do cadastro do SGT para o reduzido.
                    _pi = ctxTI.VW_PI_RED.Where(x => x.REDUZIDO == model.Reduzido).FirstOrDefault().PI;

                    //Tenta pegar agora a combinação do reduzido + PI na tabela de padrões
                    model.TamanhoPadrao = ctxTI.REGRA_PADRAO.Where(x => x.STATUS == true && x.REDUZIDO == model.Reduzido && x.ID_PROCESSO == _pi).OrderByDescending(x => x.PADRAO).ToList();

                    //Se não encontrou, tenta encontar apenas pelo reduzido
                    if (model.TamanhoPadrao == null || model.TamanhoPadrao.Count() == 0)
                    {
                        model.TamanhoPadrao = ctxTI.REGRA_PADRAO.Where(x => x.STATUS == true && x.REDUZIDO == model.Reduzido &&
                                                                            x.ID_PROCESSO == null).OrderByDescending(x => x.PADRAO).ToList();
                    }

                    //Não encontrou na tabela, tentar encontar sem o reduzido - [Tipo prod, Tec, Artigo e PI]
                    if (model.TamanhoPadrao == null || model.TamanhoPadrao.Count() == 0)
                    {
                        model.TamanhoPadrao = ctxTI.REGRA_PADRAO.Where(x => x.STATUS == true && x.TIPO_PRODUTO == "A" &&
                                                                            x.ARTIGO == model.Artigo &&
                                                                            x.TECNOLOGIA == model.Tecnologia &&
                                                                            x.ID_PROCESSO == _pi
                                                                    ).OrderByDescending(x => x.PADRAO).ToList();
                    }
                }
                if ((model.Reduzido == -2) || (model.TamanhoPadrao == null) || (model.TamanhoPadrao.Count() == 0))
                {
                    //Verifica se pegou a UM, se não procura a Unoida de Medida do itens estoque-----
                    if (model.UnidadeMedida == null)
                    {
                        VMASCARAPRODUTOACABADO objValorPadraoView = null;

                        using (var ctx = new DalutexContext())
                        {
                            objValorPadraoView = ctx.VMASCARAPRODUTOACABADO.Where(x => x.ARTIGO == model.Artigo).FirstOrDefault();
                            if (objValorPadraoView != null)
                            {
                                model.UnidadeMedida = objValorPadraoView.UM;
                                if (model.UnidadeMedida.ToUpper() == "KG")
                                {
                                    model.ValorPadrao = (decimal)Enums.ValorPadraoUnidade.Quilo;
                                }
                                else if (model.UnidadeMedida.ToUpper() == "MT")
                                {
                                    model.ValorPadrao = (decimal)Enums.ValorPadraoUnidade.Metro;
                                }
                                else
                                {
                                    ModelState.AddModelError("", "UNIDADE DE MEDIDA INVÁLIDA.");
                                }
                            }
                            else
                            {
                                ModelState.AddModelError("", "UNIDADE DE MEDIDA NÃO ENCONTRADA.");
                            }
                        }
                    }

                    //Se Não encontrou na tabela ainda, tentar encontar sem o reduzido na combinação - [Tipo prod, Tecnologia, Artigo e UM]
                    if (model.TamanhoPadrao == null || model.TamanhoPadrao.Count() == 0)
                    {
                        model.TamanhoPadrao = ctxTI.REGRA_PADRAO.Where(x => x.STATUS == true && x.TIPO_PRODUTO == "A" &&
                                                                            x.ARTIGO == model.Artigo &&
                                                                            x.TECNOLOGIA == model.Tecnologia && 
                                                                            x.UM == model.UnidadeMedida
                                                                    ).OrderByDescending(x => x.PADRAO).ToList();
                    }

                    //Se Não encontrou na tabela ainda, tentar encontar sem o reduzido na combinação - [Tipo prod, Tecnologia e UM]
                    if (model.TamanhoPadrao == null || model.TamanhoPadrao.Count() == 0)
                    {
                        model.TamanhoPadrao = ctxTI.REGRA_PADRAO.Where(x => x.STATUS == true && x.TIPO_PRODUTO == "A" &&                                                                            
                                                                            x.TECNOLOGIA == model.Tecnologia && 
                                                                            x.UM == model.UnidadeMedida &&
                                                                            x.ARTIGO == null
                                                                    ).OrderByDescending(x => x.PADRAO).ToList();
                    }

                    //Se Não encontrou na tabela ainda, tentar encontar sem o reduzido na combinação - [Tipo prod, UM]
                    if (model.TamanhoPadrao == null || model.TamanhoPadrao.Count() == 0)
                    {
                        model.TamanhoPadrao = ctxTI.REGRA_PADRAO.Where(x => x.STATUS == true && x.TIPO_PRODUTO == "A" &&
                                                                            x.UM == model.UnidadeMedida &&
                                                                            x.ARTIGO == null
                                                                      ).OrderByDescending(x => x.PADRAO).ToList();
                    }

                    //Se Não encontrou AINDA na tabela, PEGA O PRIMEIRO VALOR APENAS DO TIPO DE PRODUTO e mesma UM
                    if (model.TamanhoPadrao == null || model.TamanhoPadrao.Count() == 0)
                    {
                        model.TamanhoPadrao = ctxTI.REGRA_PADRAO.Where(x => x.STATUS == true && x.TIPO_PRODUTO == "A" && 
                                                                            //x.TECNOLOGIA == "X" &&  
                                                                            //x.ARTIGO == null &&
                                                                            x.UM == model.UnidadeMedida
                                                                       ).OrderByDescending(x => x.PADRAO).ToList();
                    }
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InserirNoCarrinho(InserirNoCarrinhoViewModel model)
        {
            bool hasErrors = false;
            ViewBag.POGReduzido = model.Reduzido;
            try
            {
                if (ModelState.IsValid)
                {
                    if (model.Tipo != Enums.ItemType.Reserva)
                    {
                        if ((model.IDTipoPedido != (int)Enums.TiposPedido.AMOSTRA) && (model.IDTipoPedido != (int)Enums.TiposPedido.PILOTAGEM))
                        {
                            model.Quantidade = model.IDTamanhoPadrao.GetValueOrDefault() * model.Pecas;
                        }
                        
                        #region Validação dos campos qtde
                        //TODO: 
                        // VERIFICAR A VALIDAÇÃO DESTE CAMPO. DE ACORDO COM A ALTERAÇÃO, AGORA EXISTEM VARIOS TAMANHOS PADROES PARA SELECIONAR UM (APENAS).
                        // ESTE É O VALOR DO COMBO QUE SERÁ UTILIZADO PARA FAZER A CONTA (TAMANHO PADRÃO * QTDE DE PEÇAS INFORMADOS .
                        // ESTE VALOR NÃO PODE SER SETADO PELO GET. NO GET, PODE SER DEFINIDO UM DEFAULT PRA SETAR O COMBO COM ESTE VALOR, POREM NO POST, O 
                        // VALOR SELECIONADO DEVE SER DEVOLVIDO PARA VALIDAÇÃO AKI.

                        if ((model.IDTipoPedido != (int)Enums.TiposPedido.AMOSTRA) && (model.IDTipoPedido != (int)Enums.TiposPedido.PILOTAGEM))
                        {
                            if (model.IDTamanhoPadrao <= 0)
                            {
                                ModelState.AddModelError("", "TAMANHO PADRÃO NÃO SELECIONADO.");
                                hasErrors = true;
                            }
                        }
                        if (model.IDTipoPedido == 0 && model.Pecas <= 0)
                        {
                            ModelState.AddModelError("", "CAMPO \"PEÇAS\" NÃO PODE SER MENOR OU IGUAL A ZERO.");
                            hasErrors = true;
                        }
                        if (model.IDTipoPedido != 0 && model.Quantidade <= 0)
                        {
                            ModelState.AddModelError("", "CAMPO \"" + model.UnidadeMedida + "\" NÃO PODE SER MENOR OU IGUAL A ZERO.");
                            hasErrors = true;
                        }
                        if (model.Preco <= 0)
                        {
                            ModelState.AddModelError("", "CAMPO \"PREÇO\" NÃO PODE SER MENOR OU IGUAL A ZERO.");
                            hasErrors = true;
                        }
                        #endregion

                        using (var ctx = new TIDalutexContext())
                        {
                            decimal QtdeMaxima = 0;
                            decimal QtdeMinima = 0;

                            REGRAS_QTD_PEDIDO objMinMax=null;

                            int ID_GRUPO_COL = 0;

                            CONFIG_GERAL objColPocket = ctx.CONFIG_GERAL.Find((int)Enums.TipoColecaoEspecial.Pocket);

                            if (model.IDColecao == objColPocket.INT1.ToString())
                                ID_GRUPO_COL = (int)Enums.GrupoColecoes.Pocket;
                            else if (model.IDColecao == ((int)Enums.TipoColecaoEspecial.Exclusivos).ToString())
                                ID_GRUPO_COL = (int)Enums.GrupoColecoes.Exclusivos;
                            else
                                ID_GRUPO_COL = (int)Enums.GrupoColecoes.Colecao;


                            // -- oda -- 04/11/2015 -- nova regra de tamanho Min e Max -------------------------------------------------------------------------------------
                            model.IDGrupoColecao = ID_GRUPO_COL;

                            int _piOld = 0;

                            //se for troca de tecnologia, então tento pegar o reduzido do item na tecnologia original pelo artigo selecionado
                            if (model.Tecnologia != model.TecnologiaOriginal)
                            {
                                using (var ctxDalutex = new DalutexContext())
                                {   //PEGAR O REDUZIDO DO ORIGINAL ----
                                    VMASCARAPRODUTOACABADO objReduzido = ctxDalutex.VMASCARAPRODUTOACABADO.Where(
                                              x => x.ARTIGO == model.Artigo
                                                && x.DESENHO == model.Desenho
                                                && x.VARIANTE == model.Variante
                                                && x.MAQUINA == model.TecnologiaOriginal
                                            ).FirstOrDefault();
                                    //se encontrou o reduzido, procuro o PI ----
                                    if (objReduzido != null)
                                    {
                                        _piOld = ctx.VW_PI_RED.Where(x => x.REDUZIDO == objReduzido.CODIGO_REDUZIDO).FirstOrDefault().PI;
                                    }
                                }
                            }

                            int _pi = 0;

                            if (model.Reduzido > -2) //se tem reduzido (o item novo, na tec atual) --------
                            {
                                // pega o PI do cadastro do SGT para o reduzido.                         
                                _pi = ctx.VW_PI_RED.Where(x => x.REDUZIDO == model.Reduzido).FirstOrDefault().PI;
                            }

                            //procura as qtdes max e min, pela regra 1.1: [TEC_ORIGEM + PROCESSO_OR + TEC_DESTI + PROC_DEST + GRUPO_COL + TIPO_PED + UM]
                            if ( _pi > 0 )//se tem PI Origem
                            {
                                var qryQtdeMinMax =
                                    from Qtde in ctx.REGRAS_QTD_PEDIDO
                                    where
                                           Qtde.TECNOLOGIA == model.TecnologiaOriginal.Substring(0,1)
                                        && Qtde.PROCESSO == _pi
                                        && Qtde.TECNOLOGIA_DESTINO == model.Tecnologia
                                        && Qtde.PROCESSO_DESTINO == _piOld
                                        && Qtde.GRUPO_COLECAO == ID_GRUPO_COL
                                        && Qtde.TIPO_PEDIDO == model.IDTipoPedido
                                        && Qtde.UM == model.UnidadeMedida
                                        && Qtde.EXCLUIDO == false
                                    select
                                        Qtde;

                                objMinMax = qryQtdeMinMax.FirstOrDefault();
                                                        
                                if (objMinMax != null)
                                {
                                    QtdeMaxima = objMinMax.QTD_MAX_VAR;
                                    QtdeMinima = objMinMax.QTD_MIN_VAR;
                                }
                            }
                            else if ((_pi > 0) && (objMinMax != null))//se não encontrou, procura as qtdes max e min, pela regra 1.2: [TEC_ORIGEM + PROCESSO_OR + TEC_DESTI + PROC_DEST + TIPO_PED + UM]
                            {
                                var qryQtdeMinMax =
                                from Qtde in ctx.REGRAS_QTD_PEDIDO
                                where
                                       Qtde.TECNOLOGIA == model.TecnologiaOriginal.Substring(0, 1)
                                    && Qtde.PROCESSO == _pi 
                                    && Qtde.TECNOLOGIA_DESTINO == model.Tecnologia
                                    && Qtde.PROCESSO_DESTINO == _piOld
                                    && Qtde.TIPO_PEDIDO == model.IDTipoPedido
                                    && Qtde.UM == model.UnidadeMedida
                                    && Qtde.EXCLUIDO == false
                                select
                                    Qtde;

                                objMinMax = qryQtdeMinMax.FirstOrDefault();
                            }                            
                            if (objMinMax != null)
                            {
                                QtdeMaxima = objMinMax.QTD_MAX_VAR;
                                QtdeMinima = objMinMax.QTD_MIN_VAR;
                            }
                            else if (_pi > 0)//se não encontrou, procura as qtdes max e min, pela regra 1.3: [TEC_ORIGEM + PROCESSO_OR + TEC_DESTI + TIPO_PED + UM]
                            {
                                var qryQtdeMinMax =
                                from Qtde in ctx.REGRAS_QTD_PEDIDO
                                where
                                       Qtde.TECNOLOGIA == model.TecnologiaOriginal.Substring(0, 1)
                                    && Qtde.PROCESSO == _pi
                                    && Qtde.TECNOLOGIA_DESTINO == model.Tecnologia
                                    && Qtde.TIPO_PEDIDO == model.IDTipoPedido
                                    && Qtde.UM == model.UnidadeMedida
                                    && Qtde.EXCLUIDO == false
                                select
                                    Qtde;

                                objMinMax = qryQtdeMinMax.FirstOrDefault();
                            }
                            
                            if (objMinMax != null)
                            {
                                QtdeMaxima = objMinMax.QTD_MAX_VAR;
                                QtdeMinima = objMinMax.QTD_MIN_VAR;
                            }
                            else if (_pi > 0)//se não encontrou, procura as qtdes max e min, pela regra 1.4: [TEC_ORIGEM + PROCESSO_OR + GRUPO_COL + TIPO_PED + UM]
                            {
                                var qryQtdeMinMax =
                                from Qtde in ctx.REGRAS_QTD_PEDIDO
                                where
                                       Qtde.TECNOLOGIA == model.TecnologiaOriginal.Substring(0, 1)
                                    && Qtde.PROCESSO == _pi
                                    && Qtde.GRUPO_COLECAO == ID_GRUPO_COL
                                    && Qtde.TIPO_PEDIDO == model.IDTipoPedido
                                    && Qtde.UM == model.UnidadeMedida
                                    && Qtde.EXCLUIDO == false
                                select
                                    Qtde;

                                objMinMax = qryQtdeMinMax.FirstOrDefault();
                            }
                            if (objMinMax != null)
                            {
                                QtdeMaxima = objMinMax.QTD_MAX_VAR;
                                QtdeMinima = objMinMax.QTD_MIN_VAR;
                            }
                            else if (_pi > 0)//se não encontrou, procura as qtdes max e min, pela regra 1.5: [TEC_ORIGEM + PROCESSO_OR + TIPO_PED + UM]
                            {
                                var qryQtdeMinMax =
                                from Qtde in ctx.REGRAS_QTD_PEDIDO
                                where
                                       Qtde.TECNOLOGIA == model.TecnologiaOriginal.Substring(0, 1)
                                    && Qtde.PROCESSO == _pi                                    
                                    && Qtde.TIPO_PEDIDO == model.IDTipoPedido
                                    && Qtde.UM == model.UnidadeMedida
                                    && Qtde.EXCLUIDO == false
                                select
                                    Qtde;

                                objMinMax = qryQtdeMinMax.FirstOrDefault();
                            }
                            if (objMinMax != null)
                            {
                                QtdeMaxima = objMinMax.QTD_MAX_VAR;
                                QtdeMinima = objMinMax.QTD_MIN_VAR;
                            }
                            else//se não encontrou, procura as qtdes max e min, pela regra 2.1: [TEC_ORIGEM + TEC_DESTINO + GRUPO_COL + TIPO_PEDIDO + UM]
                            {
                                var qryQtdeMinMax =
                                from Qtde in ctx.REGRAS_QTD_PEDIDO
                                where
                                       Qtde.TECNOLOGIA == model.TecnologiaOriginal.Substring(0, 1)
                                    && Qtde.TECNOLOGIA_DESTINO == model.Tecnologia
                                    && Qtde.GRUPO_COLECAO == ID_GRUPO_COL 
                                    && Qtde.TIPO_PEDIDO == model.IDTipoPedido
                                    && Qtde.PROCESSO == 0
                                    && Qtde.UM == model.UnidadeMedida
                                    && Qtde.EXCLUIDO == false
                                select
                                    Qtde;

                                objMinMax = qryQtdeMinMax.FirstOrDefault();
                            }
                            if (objMinMax != null)
                            {
                                QtdeMaxima = objMinMax.QTD_MAX_VAR;
                                QtdeMinima = objMinMax.QTD_MIN_VAR;
                            }
                            else//se não encontrou, procura as qtdes max e min, pela regra 2.2: [TEC_ORIGEM + TEC_DESTINO + TIPO_PEDIDO + UM]
                            {
                                var qryQtdeMinMax =
                                from Qtde in ctx.REGRAS_QTD_PEDIDO
                                where
                                        Qtde.TECNOLOGIA == model.TecnologiaOriginal.Substring(0, 1)
                                    && Qtde.TECNOLOGIA_DESTINO == model.Tecnologia                                    
                                    && Qtde.TIPO_PEDIDO == model.IDTipoPedido
                                    && Qtde.PROCESSO == 0
                                    && Qtde.UM == model.UnidadeMedida
                                    && Qtde.EXCLUIDO == false
                                select
                                    Qtde;

                                objMinMax = qryQtdeMinMax.FirstOrDefault();
                            }
                            if (objMinMax != null)
                            {
                                QtdeMaxima = objMinMax.QTD_MAX_VAR;
                                QtdeMinima = objMinMax.QTD_MIN_VAR;
                            }
                            else //se não encontrou, procura as qtdes max e min, pela regra 3.1: [TEC_DESTINO + GRUPO_COL + TIPO_PEDIDO + UM]
                            {
                                var qryQtdeMinMax =
                                from Qtde in ctx.REGRAS_QTD_PEDIDO
                                where
                                       Qtde.TECNOLOGIA_DESTINO == model.Tecnologia
                                        && Qtde.GRUPO_COLECAO == ID_GRUPO_COL
                                        && Qtde.TIPO_PEDIDO == model.IDTipoPedido
                                        && Qtde.PROCESSO == 0
                                        && Qtde.UM == model.UnidadeMedida
                                        && Qtde.EXCLUIDO == false
                                select
                                    Qtde;

                                objMinMax = qryQtdeMinMax.FirstOrDefault();
                            }
                            if (objMinMax != null)
                            {
                                QtdeMaxima = objMinMax.QTD_MAX_VAR;
                                QtdeMinima = objMinMax.QTD_MIN_VAR;
                            }
                            else//se não encontrou, procura as qtdes max e min, pela regra 3.2: [TEC_DESTINO + TIPO_PEDIDO + UM]
                            {
                                var qryQtdeMinMax =
                                from Qtde in ctx.REGRAS_QTD_PEDIDO
                                where
                                       Qtde.TECNOLOGIA_DESTINO == model.Tecnologia
                                       && Qtde.TIPO_PEDIDO == model.IDTipoPedido
                                       && Qtde.PROCESSO == 0
                                       && Qtde.UM == model.UnidadeMedida
                                       && Qtde.EXCLUIDO == false
                                select
                                    Qtde;

                                objMinMax = qryQtdeMinMax.FirstOrDefault();
                            }
                            if (objMinMax != null)
                            {
                                QtdeMaxima = objMinMax.QTD_MAX_VAR;
                                QtdeMinima = objMinMax.QTD_MIN_VAR;
                            }

                            if (objMinMax == null)
                            {
                                ModelState.AddModelError("", "QUANTIDADES MÍNIMAS E MÁXIMAS POR VARIANTE NÃO CADASTRADO PARA ESTE ITEM. ENTRAR EM CONTATO COM DEPTO. COMERCIAL: ");
                                hasErrors = true;
                            }
                            else
                            {
                                if (model.Quantidade < QtdeMinima)
                                {
                                    ModelState.AddModelError("", "A QUANTIDADE MÍNIMA POR VARIANTE NÃO PODE SER MENOR QUE: " + QtdeMinima.ToString());
                                    hasErrors = true;
                                }
                                if (model.Quantidade > QtdeMaxima)
                                {
                                    ModelState.AddModelError("", "A QUANTIDADE MÁXIMA POR VARIANTE NÃO PODE SER MAIOR QUE: " + QtdeMaxima.ToString());
                                    hasErrors = true;
                                }
                            }
                            // -- oda -- 04/11/2015 -- nova regra de tamanho Min e Max -------------------------------------------------------------------------------------



                            //regra OLD -------------------------------------------------------------------------------------------------------------
                            //var queryMinMax =
                            //    from plqt in ctx.PED_LINK_QUANTD_TIPO
                            //    join cdt in ctx.CONTROLE_DESENV_TECNOLOGIA on plqt.ID_TECNOLOGIA equals cdt.ID_TEC
                            //    where
                            //        cdt.DESC_TEC == model.TecnologiaPorExtenso
                            //        && plqt.TIPO_PEDIDO == model.IDTipoPedido
                            //        && plqt.ARTIGO == model.Artigo
                            //        && plqt.ID_GRUPO_COL == ID_GRUPO_COL
                            //    select
                            //        plqt;
                            //PED_LINK_QUANTD_TIPO objMinMax = queryMinMax.FirstOrDefault();

                            //if (objMinMax == null)
                            //{
                            //    objMinMax = new PED_LINK_QUANTD_TIPO();
                            //    objMinMax.QTDE_MIN = 1;
                            //    objMinMax.QTDE_MAX = 999999;
                            //}

                            //if (model.IDTipoPedido == 0)
                            //{
                            //    if (model.Pecas < objMinMax.QTDE_MIN)
                            //    {
                            //        ModelState.AddModelError("", "A QUANTIDADE MÍNIMA DE PEÇAS NÃO PODE SER MENOR QUE: " + objMinMax.QTDE_MIN.ToString());
                            //        hasErrors = true;
                            //    }
                            //    if (model.Pecas > objMinMax.QTDE_MAX)
                            //    {
                            //        ModelState.AddModelError("", "A QUANTIDADE MÁXIMA DE PEÇAS NÃO PODE SER MAIOR QUE: " + objMinMax.QTDE_MAX.ToString());
                            //        hasErrors = true;
                            //    }
                            //}
                            //else
                            //{
                            //    if (model.Quantidade < objMinMax.QTDE_MIN)
                            //    {
                            //        ModelState.AddModelError("", "A QUANTIDADE MÍNIMA NÃO PODE SER MENOR QUE: " + objMinMax.QTDE_MIN.ToString());
                            //        hasErrors = true;
                            //    }
                            //    if (model.Quantidade > objMinMax.QTDE_MAX)
                            //    {
                            //        ModelState.AddModelError("", "A QUANTIDADE MÁXIMA NÃO PODE SER MAIOR QUE: " + objMinMax.QTDE_MAX.ToString());
                            //        hasErrors = true;
                            //    }
                            //}
                        }
                    }

                    if (hasErrors)
                    {
                        if (model.Tipo != Enums.ItemType.Reserva)
                        {
                            if (base.Session_Carrinho == null || base.Session_Carrinho.IDTipoPedido < 0)
                            {
                                model.ObterTipoPedido = "S";
                                this.CarregarTiposPedidos(model);
                            }
                        }
                        else
                        {
                            model.ObterTipoPedido = "N";
                        }

                        CarregarTamanhosPadrao(model);

                        return View(model);
                    } 

                    if (base.Session_Carrinho == null)
                    {
                        base.Session_Carrinho = new ConclusaoPedidoViewModel();
                        base.Session_Carrinho.Itens = new List<InserirNoCarrinhoViewModel>();
                    }
                    else
                    {
                        if (base.Session_Carrinho.Itens == null)
                        {
                            base.Session_Carrinho.Itens = new List<InserirNoCarrinhoViewModel>();
                        }
                    }

                    if (model.IDTipoPedido >= default(int))
                        base.Session_Carrinho.IDTipoPedido = model.IDTipoPedido;

                    if (model.IDTipoPedido != (int)Enums.TiposPedido.VENDA)                        
                        model.Pecas = 1;

                    model.ValorTotalItem = model.Quantidade * model.Preco;


                    if (model.Tipo != Enums.ItemType.Reserva)
                    {
                        //adicionar valor para "Farol" ------------ oda -- 05/08/2015 -----------
                        using (var ctx = new TIDalutexContext())
                        {
                            VW_FAROL objFarol = ctx.VW_FAROL.Where(x => x.ARTIGO == model.Artigo && x.DESENHO == model.Desenho && x.VARIANTE == model.Variante).FirstOrDefault();

                            if (objFarol != null)
                                model.Farol = objFarol.FAROL;
                            else
                                model.Farol = 0;
                        }
                    }

                    
                    if (model.Tipo != Enums.ItemType.Reserva)
                    {
                        using (var ctx = new TIDalutexContext())
                        {
                            int iID_DISP = ctx.DISPONIBILIDADE_MALHA
                                                    .OrderByDescending(x => x.ID_DISP)
                                                    .First()
                                                    .ID_DISP;

                            DISPONIBILIDADE_MALHA objDisponibilidade = ctx.DISPONIBILIDADE_MALHA
                                                    .Where(x => x.ARTIGO == model.Artigo && x.MAQUINA == model.Tecnologia && x.ID_DISP == iID_DISP)
                                                    .FirstOrDefault();

                            if (objDisponibilidade != null && objDisponibilidade.DISPONIBILIDADE_PCP != null)
                                model.DataEntregaItem = (DateTime)objDisponibilidade.DISPONIBILIDADE_PCP;
                            else
                                model.DataEntregaItem = DateTime.Today.AddYears(1);

                            if (base.Session_Carrinho.DataEntrega < model.DataEntregaItem)
                                base.Session_Carrinho.DataEntrega = model.DataEntregaItem;
                        }
                    }

                    if (model.Modo == "I")//Inclusão
                    {
                        if (base.Session_Carrinho.Itens.Contains(model))
                        {
                            ModelState.AddModelError("", "Este item já foi incluído no carrinho.");
                            if (base.Session_Carrinho == null || base.Session_Carrinho.IDTipoPedido < 0)
                            {
                                model.ObterTipoPedido = "S";
                                this.CarregarTiposPedidos(model);                                
                            }
                            this.CarregarTamanhosPadrao(model);

                            return View(model);
                        }

                        if (base.Session_Carrinho.Itens.Count == 0)
                            model.NumeroSequencial = 1;
                        else
                            model.NumeroSequencial = base.Session_Carrinho.Itens.Max(x => x.NumeroSequencial) + 1;

                        base.Session_Carrinho.Itens.Add(model);

                        if (model.Tipo == Enums.ItemType.Estampado || model.Tipo == Enums.ItemType.ValidacaoReserva)
                            return RedirectToAction("ArtigosDisponiveis", "Pedido",
                                new
                                {
                                    desenho = model.Desenho,
                                    variante = model.Variante,
                                    idcolecao = model.IDColecao,
                                    nmcolecao = model.NMColecao,
                                    pagina = model.Pagina,
                                    pedidoreserva = model.PedidoReserva,
                                    idvariante = model.IDVariante,
                                    itempedidoreserva = model.ItemPedidoReserva,
                                    tipo = (int)model.Tipo
                                });
                        else if (model.Tipo == Enums.ItemType.Liso)
                            return RedirectToAction("Lisos", "Pedido", new { idcolecao = model.IDColecao, nmcolecao = model.NMColecao, pagina = model.Pagina });
                        else if (model.Tipo == Enums.ItemType.Reserva)
                            return RedirectToAction("ItensParaReserva", "Pedido", new { pagina = model.Pagina });
                        else
                            return RedirectToAction("Index", "Home");                      
                    }
                    else
                    {
                        int index = base.Session_Carrinho.Itens.FindIndex(x => x.Equals(model));
                        if (index < 0)
                        {
                            ModelState.AddModelError("", "Este item não foi encontrado no carrinho para alteração.");
                            if (base.Session_Carrinho == null || base.Session_Carrinho.IDTipoPedido < 0)
                            {
                                model.ObterTipoPedido = "S";
                                this.CarregarTiposPedidos(model);
                            }
                            this.CarregarTamanhosPadrao(model);
                            return View(model);
                        }

                        base.Session_Carrinho.Itens[index] = model;
                        return RedirectToAction("Carrinho", "Pedido");
                    }
                }
                else
                {
                    if (base.Session_Carrinho == null || base.Session_Carrinho.IDTipoPedido < 0)
                    {
                        model.ObterTipoPedido = "S";
                        this.CarregarTiposPedidos(model);
                    }
                    CarregarTamanhosPadrao(model);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                base.Handle(ex);
            }
            
            if (base.Session_Carrinho == null || base.Session_Carrinho.IDTipoPedido < 0)
            {
                model.ObterTipoPedido = "S";
                this.CarregarTiposPedidos(model);
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult ExcluirItemCarrinho(InserirNoCarrinhoViewModel model)
        {
            if (base.Session_Carrinho != null && base.Session_Carrinho.Itens != null)
            {
                if (base.Session_Carrinho.Itens.Remove(model))
                {
                    if (base.Session_Carrinho.Itens.Count == default(int))
                    {
                        base.Session_Carrinho = null;
                    }
                    return RedirectToAction("Carrinho");
                }
                else
                {
                    return RedirectToAction("Error", new { message = "Este item não foi encontrado no carrinho para excluir.", title = "EXCLUSÃO DO CARRINHO" });
                }
            }
            else
            {
                return RedirectToAction("Error", new { message = "Não há itens no carrinho para excluir.", title = "EXCLUSÃO DO CARRINHO" });
            }
        }

        public ActionResult Carrinho()
        {
            if (base.Session_Carrinho == null)
            {
                return View();
            }

            ViewBag.Carrinho = base.Session_Carrinho;
            ViewBag.UrlDesenhos = ConfigurationManager.AppSettings["PASTA_DESENHOS"];
            ViewBag.UrlReservas = ConfigurationManager.AppSettings["PASTA_RESERVAS"];

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public void AtualizaCompose(string compose, string sequencial)
        {
            int iSequencial = int.Parse(sequencial);
            InserirNoCarrinhoViewModel objAtualizar = base.Session_Carrinho.Itens.Where(x => x.NumeroSequencial == iSequencial).First();
            objAtualizar.Compose = int.Parse(compose);
        }

        public ActionResult EsvaziarCarrinho()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EsvaziarCarrinho(object model)
        {
            base.Session_Carrinho = new ConclusaoPedidoViewModel();
            return RedirectToAction("Index", "Home");
        }

        private ConclusaoPedidoViewModel ConclusaoPedidoCarregarListas(ConclusaoPedidoViewModel model)
        {
            if (model.IDTipoPedido != (int)Enums.TiposPedido.RESERVA)
            {
                model.QualidadeComercial = new List<KeyValuePair<string, string>>();
                model.QualidadeComercial.Add(new KeyValuePair<string, string>(Enums.QualidadeComercial.A.ToString(), Enums.QualidadeComercial.A.ToString()));
                model.QualidadeComercial.Add(new KeyValuePair<string, string>(Enums.QualidadeComercial.B.ToString(), Enums.QualidadeComercial.B.ToString()));
                model.QualidadeComercial.Add(new KeyValuePair<string, string>(Enums.QualidadeComercial.C.ToString(), Enums.QualidadeComercial.C.ToString()));

                using (DalutexContext ctxDalutex = new DalutexContext())
                {
                    model.Moedas = ctxDalutex.CADASTRO_MOEDAS.ToList();
                    model.ViasTransporte = ctxDalutex.COML_VIASTRANSPORTE.ToList();
                    model.Fretes = ctxDalutex.COML_TIPOSFRETE.ToList();
                    model.CanaisVenda = ctxDalutex.CANAIS_VENDA.ToList();
                    model.GerentesVenda = ctxDalutex.COML_GERENCIAS.Where(x => x.CANALVENDA == (int)Enums.CanaisVenda.TELEVENDAS).ToList();
                }

                using (TIDalutexContext ctxTI = new TIDalutexContext())
                {
                    model.LocaisVenda = ctxTI.LOCALVENDA.ToList();
                    model.TiposAtendimento = ctxTI.PRE_PEDIDO_ATEND.ToList();
                    model.CondicoesPagto = ctxTI.VW_CONDICAO_PGTO.ToList();
                }

                //if (base.Session_Carrinho != null)
                //    model.Itens = base.Session_Carrinho.Itens;


                //TODO: ver com cassiano alterações na forma de calcular o total do item:::::

                //foreach (InserirNoCarrinhoViewModel item in model.Itens)
                //{
                //    model.TotalPedido += item.ValorTotalItem;
                //}
            }

            return model;
        }

        public ActionResult ConclusaoPedido(string idtransportadora, string idclienteFatura)
        {
            ConclusaoPedidoViewModel model = new ConclusaoPedidoViewModel();
           
            if (Session_Carrinho != null)
            {
                model.IDTipoPedido = base.Session_Carrinho.IDTipoPedido;
                if (base.Session_Carrinho.Itens != null)
                {
                    base.Session_Carrinho.Itens.ForEach(delegate(InserirNoCarrinhoViewModel item)
                    {
                        model.TotalPedido = base.Session_Carrinho.TotalPedido += item.ValorTotalItem;
                    });
                }

                if (!string.IsNullOrWhiteSpace(idtransportadora))
                {
                    Session_Carrinho.IDTransportadora = int.Parse(idtransportadora);
                }

                if (!string.IsNullOrWhiteSpace(idclienteFatura))
                {
                    Session_Carrinho.IDClienteFatura = int.Parse(idclienteFatura);
                }

                ConclusaoPedidoCarregarListas(model);
                ViewBag.CarrinhoVazio = false;
            }
            else
            {
                ViewBag.CarrinhoVazio = true;
            }

            return View(model);
        }

        public JsonResult ObterItensCarrinho()
        {
            return Json(base.Session_Carrinho.Itens.OrderBy(x => x.Compose), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConclusaoPedido(ConclusaoPedidoViewModel model)
        {
            try
            {
                if (model.IDTipoPedido == (int)Enums.TiposPedido.RESERVA)
                {
                    model.IDQualidadeComercial = Enums.QualidadeComercial.A.ToString();
                    model.IDCondicoesPagto = (int)Enums.CondicoesPagamento.CORTESIA;                    
                }

                if (Session_Carrinho == null)
                {
                    ViewBag.CarrinhoVazio = true;
                    return View(model);
                }
                else
                {
                    ViewBag.CarrinhoVazio = false;
                }

                if (ModelState.IsValid)
                {
                    #region Validações

                    if (Session_Carrinho.Itens == null || Session_Carrinho.Itens.Count == 0)
                    {
                        ModelState.AddModelError("", "Não é permitido concluir o pedido sem itens.");
                        this.ConclusaoPedidoCarregarListas(model);
                        return View(model);
                    }

                    foreach (InserirNoCarrinhoViewModel item in Session_Carrinho.Itens)
                    {
                        model.TotalPedido += item.ValorTotalItem;
                    }

                    if (model.IDTipoPedido != (int)Enums.TiposPedido.RESERVA && Session_Carrinho.Itens.Exists(x => x.Reduzido == 0))
                    {
                        ModelState.AddModelError("", "Este carrinho possuem itens com o código reduzido zerado. Favor entrar em contato com o TI.");
                        this.ConclusaoPedidoCarregarListas(model);
                        return View(model);
                    }

                    bool hasErrors = false;

                    if (model.IDTipoPedido != (int)Enums.TiposPedido.RESERVA)
                    {

                        if (model.IDCondicoesPagto <= 0 || model.IDCondicoesPagto == null)
                        {
                            ModelState.AddModelError("", "Por favor informe a condição de pagamento.");
                            hasErrors = true;
                        }
                        if (model.IDMoedas < 0 || model.IDMoedas == null)
                        {
                            ModelState.AddModelError("", "Por favor informe a moeda.");
                            hasErrors = true;
                        }
                        if (model.IDViasTransporte <= 0 || model.IDViasTransporte == null)
                        {
                            ModelState.AddModelError("", "Por favor informe a via de transporte.");
                            hasErrors = true;
                        }
                        if (model.IDFretes <= 0)
                        {
                            ModelState.AddModelError("", "Por favor informe o tipo de frete.");
                            hasErrors = true;
                        }
                        if (model.IDCanaisVenda <= 0)
                        {
                            ModelState.AddModelError("", "Por favor informe o canal de venda.");
                            hasErrors = true;
                        }
                        if (model.IDTiposAtendimento <= 0 || model.IDTiposAtendimento == null)
                        {
                            ModelState.AddModelError("", "Por favor informe o tipo de atendimento.");
                            hasErrors = true;
                        }
                        if (string.IsNullOrWhiteSpace(model.IDQualidadeComercial))
                        {
                            ModelState.AddModelError("", "Por favor informe a qualidade comercial.");
                            hasErrors = true;
                        }

                        if (hasErrors)
                        {
                            this.ConclusaoPedidoCarregarListas(model);
                            return View(model);
                        }

                        if (base.Session_Carrinho.IDTipoPedido.Equals((int)Enums.TiposPedido.VENDA)
                            && !model.IDCanaisVenda.Equals((int)Enums.CanaisVenda.TELEVENDAS)
                            && !model.IDCondicoesPagto.Equals((int)Enums.CondicoesPagamento.CORTESIA))
                        {

                            int numParcelas = 0;
                            using (var ctx = new TIDalutexContext())
                            {
                                numParcelas = ctx.VW_CONDICAO_PGTO.Find(model.IDCondicoesPagto).PARCELAS;
                            }

                            int fatorMultiplicacao = 0;
                            decimal valorMinimoParcelas = decimal.Parse(ConfigurationManager.AppSettings["VALOR_PARCELA_MINIMA"]);

                            if (model.IDQualidadeComercial == Enums.QualidadeComercial.A.ToString())
                            {
                                fatorMultiplicacao = (int)Enums.FatorMultiplicacaoQualidadeComercial.A;
                            }
                            else if (model.IDQualidadeComercial == Enums.QualidadeComercial.B.ToString())
                            {
                                fatorMultiplicacao = (int)Enums.FatorMultiplicacaoQualidadeComercial.B;
                            }
                            else if (model.IDQualidadeComercial == Enums.QualidadeComercial.C.ToString())
                            {
                                fatorMultiplicacao = (int)Enums.FatorMultiplicacaoQualidadeComercial.C;
                            }

                            if (model.IDTiposAtendimento.Equals((int)Enums.TiposAtendimento.EstampaCompleta))
                            {
                                List<KeyValuePair<string, decimal>> lstConsolidada = base.Session_Carrinho.Itens
                                    .GroupBy(g => g.Desenho)
                                    .Select(consolidado => new KeyValuePair<string, decimal>(consolidado.First().Desenho, consolidado.Sum(s => s.ValorTotalItem)))
                                    .ToList();

                                bool isValid = true;

                                foreach (KeyValuePair<string, decimal> item in lstConsolidada)
                                {
                                    if (((item.Value / fatorMultiplicacao) / numParcelas) < valorMinimoParcelas)
                                    {
                                        ModelState.AddModelError("", "Valor mínimo das parcelas é inferior a " + valorMinimoParcelas.ToString("C") + " para o desenho: " + item.Key);
                                        isValid = false;
                                    }
                                }

                                if (!isValid)
                                {
                                    hasErrors = true;
                                }
                            }
                            else if (model.IDTiposAtendimento.Equals((int)Enums.TiposAtendimento.PedidoCompleto))
                            {
                                if (((model.TotalPedido / fatorMultiplicacao) / numParcelas) < valorMinimoParcelas)
                                {
                                    ModelState.AddModelError("", "Valor mínimo das parcelas é inferior a " + valorMinimoParcelas.ToString("C"));
                                    hasErrors = true;
                                }
                            }
                            else if (model.IDTiposAtendimento.Equals((int)Enums.TiposAtendimento.CompletoPorArtigo))
                            {
                                List<KeyValuePair<string, decimal>> lstConsolidada = base.Session_Carrinho.Itens
                                    .GroupBy(g => g.Artigo)
                                    .Select(consolidado => new KeyValuePair<string, decimal>(consolidado.First().Artigo, consolidado.Sum(s => s.ValorTotalItem)))
                                    .ToList();

                                bool isValid = true;

                                foreach (KeyValuePair<string, decimal> item in lstConsolidada)
                                {
                                    if (((item.Value / fatorMultiplicacao) / numParcelas) < valorMinimoParcelas)
                                    {
                                        ModelState.AddModelError("", "Valor mínimo das parcelas é inferior a " + valorMinimoParcelas.ToString("C") + " para o artigo: " + item.Key);
                                        isValid = false;
                                    }
                                }

                                if (!isValid)
                                {
                                    hasErrors = true;
                                }
                            }
                            else if (model.IDTiposAtendimento.Equals((int)Enums.TiposAtendimento.PedidoIncompleto))
                            {
                                bool isValid = true;

                                foreach (InserirNoCarrinhoViewModel item in base.Session_Carrinho.Itens)
                                {
                                    if (((item.ValorTotalItem / fatorMultiplicacao) / numParcelas) < valorMinimoParcelas)
                                    {
                                        ModelState.AddModelError("", "Valor mínimo das parcelas é inferior a " + valorMinimoParcelas.ToString("C") + " para o item: Desenho=" + item.Desenho + " Variante=" + item.Variante);
                                        isValid = false;
                                    }
                                }

                                if (!isValid)
                                {
                                    hasErrors = true;
                                }
                            }

                        }

// -- oda -- 05/112015 --- regra de validação de qtde por desenho ------------------------------------------------------------------------------------------------------------------
                        using (var ctx = new TIDalutexContext())
                        {
                            if (model.IDTipoPedido == (int)Enums.TiposPedido.VENDA)
                            {
                                //foreach (InserirNoCarrinhoViewModel item in base.Session_Carrinho.Itens)
                                List<KeyValuePair<string, decimal>> lstGrupoDesenho = base.Session_Carrinho.Itens
                                  .GroupBy(g => g.Desenho + g.UnidadeMedida + g.Tecnologia.Substring(0, 1) + g.IDGrupoColecao + g.TecnologiaOriginal)
                                  .Select(consolidado => new KeyValuePair<string, decimal>(
                                      consolidado.First().Desenho + consolidado.First().UnidadeMedida + consolidado.First().Tecnologia.Substring(0, 1) + consolidado.First().IDGrupoColecao + consolidado.First().TecnologiaOriginal, 
                                      consolidado.Sum(s => s.Quantidade))).ToList();

                                //Desenho       = 0,4    A616MTD2CILINDRO, 400
                                //UnidadeMedida = 4,2 
                                //Tecnologia    = 6,1  
                                //IDColecao     = 7,1
                                //tec original  = 

                                bool isValidDes = true;

                                decimal MinValue = 0;
                                decimal MaxValue = 999999;

                                REGRAS_QTD_PEDIDO objMinMax = new REGRAS_QTD_PEDIDO();
                                objMinMax = null;

                                decimal idGrCol = 0;

                                foreach (KeyValuePair<string, decimal> item in lstGrupoDesenho)
                                {
                                    idGrCol = decimal.Parse(item.Key.Substring(7, 1));

                                    //procura as qtdes max e min, pela regra 2.1: [TEC_ORIGEM + TEC_DESTI + GRUPO_COL + TIPO_PED + UM]
                                    var qryQtdeMinMax =
                                        from Qtde in ctx.REGRAS_QTD_PEDIDO
                                        where
                                               Qtde.TECNOLOGIA_DESTINO == item.Key.Substring(6, 1)
                                            && Qtde.TECNOLOGIA == item.Key.Substring(8, 1) 
                                            && Qtde.GRUPO_COLECAO == idGrCol
                                            && Qtde.TIPO_PEDIDO == model.IDTipoPedido
                                            && Qtde.UM == item.Key.Substring(4, 2)
                                            && Qtde.EXCLUIDO == false
                                        select
                                            Qtde;

                                     objMinMax = qryQtdeMinMax.FirstOrDefault();
                                        
                                     if (objMinMax != null)
                                     {
                                         MaxValue = objMinMax.QTD_MAX_DES;
                                         MinValue = objMinMax.QTD_MIN_DES;
                                     }
                                     else
                                     {
                                         //procura as qtdes max e min, pela regra 2.2: [TEC_DESTI + TEC_DESTI+ TIPO_PED + UM]
                                         var qryQtdeMinMax2 =
                                             from Qtde in ctx.REGRAS_QTD_PEDIDO
                                             where
                                                    Qtde.TECNOLOGIA_DESTINO == item.Key.Substring(6, 1)
                                                 && Qtde.TECNOLOGIA == item.Key.Substring(8, 1)   
                                                 && Qtde.GRUPO_COLECAO == 0
                                                 && Qtde.TIPO_PEDIDO == model.IDTipoPedido
                                                 && Qtde.UM == item.Key.Substring(4, 2)
                                                 && Qtde.EXCLUIDO == false
                                             select
                                                 Qtde;

                                            objMinMax = qryQtdeMinMax2.FirstOrDefault();

                                            if (objMinMax != null)
                                            {
                                                MaxValue = objMinMax.QTD_MAX_DES;
                                                MinValue = objMinMax.QTD_MIN_DES;
                                            }
                                            else
                                            {
                                                    //procura as qtdes max e min, pela regra 2.3: [TEC_DESTI + TIPO_PED + UM]
                                                    var qryQtdeMinMax3 =
                                                        from Qtde in ctx.REGRAS_QTD_PEDIDO
                                                        where
                                                            Qtde.TECNOLOGIA_DESTINO == item.Key.Substring(6, 1)
                                                            && Qtde.TIPO_PEDIDO == model.IDTipoPedido
                                                            && Qtde.UM == item.Key.Substring(4, 2)
                                                            && Qtde.EXCLUIDO == false
                                                        select
                                                            Qtde;

                                                    objMinMax = qryQtdeMinMax3.FirstOrDefault();

                                                    if (objMinMax != null)
                                                    {
                                                        MaxValue = objMinMax.QTD_MAX_DES;
                                                        MinValue = objMinMax.QTD_MIN_DES;
                                                    }
                                            }
                                    }                                                                      

                                    //CONCLUSÃO DE VALIDAÇÃO POR DESENHO ----------
                                    if (item.Value < MinValue)
                                    {
                                        ModelState.AddModelError("", "Quantidade mínima em [" + item.Key.Substring(4, 2) + "] para o desenho [ " + item.Key.Substring(0, 4) + "] em tec. [ "  + item.Key.Substring(6, 1) + " ] é " + MinValue.ToString());
                                        isValidDes = false;
                                    }
                                    else if (item.Value > MaxValue)
                                         {
                                             ModelState.AddModelError("", "Quantidade Máxima em [" + item.Key.Substring(4, 2) + "] para o desenho [" + item.Key.Substring(0, 4) + "] em tec. [ " + item.Key.Substring(6, 1) + " ] é " + MinValue.ToString());
                                                isValidDes = false;
                                         }
                                }


                                if (!isValidDes)
                                {
                                    hasErrors = true;
                                }  
                            }
                        }                                                                       
// -- oda -- 05/112015 --- regra de validação de qtde por desenho ------------------------------------------------------------------------------------------------------------------

                    }

                    if (hasErrors)
                    {
                        this.ConclusaoPedidoCarregarListas(model);
                        return View(model);
                    }

                    #endregion

                    #region Persistir na sessão

                    base.Session_Carrinho.IDQualidadeComercial = model.IDQualidadeComercial;
                    base.Session_Carrinho.IDMoedas = model.IDMoedas;
                    base.Session_Carrinho.IDCondicoesPagto = model.IDCondicoesPagto;
                    base.Session_Carrinho.IDCanaisVenda = model.IDCanaisVenda;
                    base.Session_Carrinho.IDViasTransporte = model.IDViasTransporte;
                    base.Session_Carrinho.IDFretes = model.IDFretes;
                    base.Session_Carrinho.IDTiposAtendimento = model.IDTiposAtendimento;
                    base.Session_Carrinho.Observacoes = model.Observacoes;
                    base.Session_Carrinho.PedidoCliente = model.PedidoCliente;

                    #endregion

                    if (model.IDTipoPedido != (int)Enums.TiposPedido.RESERVA)
                    {                    
                        #region Obter Preço
                        int? iColecaoAtual = 0;
                        int? iCodCondPgto = 0;

                        #region Pegar Código Condição de Pagto - Call Function Oracle
                        using(var ti_ctx = new TIDalutexContext())
                        {                                                                                                   
                            iCodCondPgto = ti_ctx.Database.SqlQuery<int>("select ti_dalutex.pega_consicao_pgto(:p0) from dual", model.IDCondicoesPagto).FirstOrDefault();

                            iColecaoAtual = int.Parse(ti_ctx.CONFIG_GERAL.Where(y => y.ID_CONFIG == (int)Enums.TipoColecaoEspecial.Atual).First().PARAMETRO1); 
                        }
                        #endregion


                        foreach (InserirNoCarrinhoViewModel item in base.Session_Carrinho.Itens)
                        {
                            using (var ctx = new TIDalutexContext())
                            {                                                                                                                 
                                if (item.Reduzido > 0)
                                {
                                    using (var ctxDlx = new DalutexContext())
                                    {
                                        decimal dReduzido = item.Reduzido;
                            
                                        var queryParametros = from vw in ctxDlx.VMASCARAPRODUTOACABADO
                                                                join co in ctxDlx.COLECOES on vw.COLECAO equals co.COLECAO
                                                                where vw.CODIGO_REDUZIDO == dReduzido
                                                                select new ParametrosPreco
                                                                {
                                                                    E_Exclusivo = vw.EXCL == "E" ? true : false,
                                                                    Comissao = co.ID_COLECAO == "POCK" ? 3 : 4,
                                                                    IDColecao = vw.COLECAO
                                                                };
                                        ParametrosPreco objParametro = queryParametros.First();                                                                                                   

                                        TABELAPRECOITEM objPreco = ctx.TABELAPRECOITEM.Where(x =>
                                                x.COLECAO == (objParametro.E_Exclusivo ? objParametro.IDColecao : iColecaoAtual)
                                                && x.QUALIDADECOMERCIAL == Session_Carrinho.IDQualidadeComercial
                                                && x.COD_COND_PAGTO == iCodCondPgto
                                                && x.EST_LISO == item.Tecnologia
                                                && x.COMISSAO == objParametro.Comissao
                                                && x.ARTIGO == item.Artigo
                                            ).FirstOrDefault();


                                        if (objPreco != null)
                                        {
                                            item.PrecoTabela = decimal.Round(objPreco.VALOR.GetValueOrDefault(), 2, MidpointRounding.ToEven);
                                        }                                    
                                    }
                                }
                                else//se não tem reduzido ----
                                {
                                    if (model.IDTipoPedido != (int)Enums.TiposPedido.RESERVA)
                                    {
                                        TABELAPRECOITEM objPreco = ctx.TABELAPRECOITEM.Where(x =>
                                                    x.COLECAO == iColecaoAtual
                                                    && x.QUALIDADECOMERCIAL == Session_Carrinho.IDQualidadeComercial
                                                    && x.COD_COND_PAGTO == iCodCondPgto
                                                    && x.EST_LISO == item.Tecnologia
                                                    && x.COMISSAO == (item.NMColecao == "POCK" ? 3 : 4)
                                                    && x.ARTIGO == item.Artigo
                                                ).FirstOrDefault();

                                        if (objPreco != null)
                                        {
                                            item.PrecoTabela = decimal.Round(objPreco.VALOR.GetValueOrDefault(), 2, MidpointRounding.ToEven);
                                        }
                                    }
                                }                                                       
                            }                       
                        }
                        #endregion
                    }
                    return RedirectToAction("Preview", "Pedido");
                }
            }
            catch (Exception ex)
            {
                base.Handle(ex);
            }

            this.ConclusaoPedidoCarregarListas(model);
            return View(model);
        }

        public ActionResult ConfirmacaoPedido(string numeropedido)
        {
            ViewBag.NumeroPedido = numeropedido;
            return View();
        }

        public EspelhoPedidoPdf EspelhoPedido(string numeropedido)
        {
            try
            {
                return new EspelhoPedidoPdf() { IDPedidoBloco = decimal.Parse(numeropedido) };
            }
            catch (Exception ex)
            {
                base.Handle(ex);
                return null;
            }
        }

        public JsonResult TotalCarrinho()
        {
            if (base.Session_Carrinho != null && base.Session_Carrinho.Itens != null && base.Session_Carrinho.Itens.Count > 0)
                return Json(base.Session_Carrinho.Itens.Count, JsonRequestBehavior.AllowGet);
            else
                return Json("", JsonRequestBehavior.AllowGet);
        }

        public ActionResult Preview()
        {
            PreviewViewModel model = new PreviewViewModel();

            model.Carrinho = base.Session_Carrinho;
            model.UrlDesenhos = ConfigurationManager.AppSettings["PASTA_DESENHOS"];
            model.UrlReservas = ConfigurationManager.AppSettings["PASTA_RESERVAS"];
            model.Observacoes = Session_Carrinho.Observacoes;
            model.QualidadeCom = Session_Carrinho.IDQualidadeComercial;
            
            if (base.Session_Carrinho.IDTipoPedido != (int)Enums.TiposPedido.RESERVA)
            {            
                using (var ctx = new DalutexContext())
                {
                    model.Representante = ctx.REPRESENTANTES.Find(Session_Carrinho.IDRepresentante).NOME;
                    model.TipoPedido = ctx.COML_TIPOSPEDIDOS.Where(x => x.TIPOPEDIDO == Session_Carrinho.IDTipoPedido).First().DESCRICAO;
                              
                    model.Transportadora = ctx.TRANSPORTADORAS.Find(Session_Carrinho.IDTransportadora).NOME;                   
                    model.CondPagto = ctx.COML_CONDICOESPAGAME.Where(x => x.CONDICAO == Session_Carrinho.IDCondicoesPagto).First().DESCRICAO;
                    model.Frete = ctx.COML_TIPOSFRETE.Where(x => x.TIPOFRETE == Session_Carrinho.IDFretes).First().DESCRICAO;                
                }            
           
                using (var ctx = new TIDalutexContext())
                {
                    model.ClienteEntrega = ctx.VW_CLIENTE_TRANSP.Where(x => x.ID_CLIENTE == Session_Carrinho.IDClienteEntrega).First().NOME;
                    model.ClienteFatura= ctx.VW_CLIENTE_TRANSP.Where(x => x.ID_CLIENTE == Session_Carrinho.IDClienteFatura).First().NOME;
                    model.TipoAtendimento = ctx.PRE_PEDIDO_ATEND.Where(x => x.COD_ATEND == Session_Carrinho.IDTiposAtendimento).First().DESCRI_ATEND;               
                }
            }
            else
            {
                using (var ctx = new DalutexContext())
                {
                    model.Representante = ctx.REPRESENTANTES.Find(Session_Carrinho.IDRepresentante).NOME;
                    model.TipoPedido = ctx.COML_TIPOSPEDIDOS.Where(x => x.TIPOPEDIDO == Session_Carrinho.IDTipoPedido).First().DESCRICAO;
                }

                using (var ctx = new TIDalutexContext())
                {
                    model.ClienteFatura = ctx.VW_CLIENTE_TRANSP.Where(x => x.ID_CLIENTE == Session_Carrinho.IDClienteFatura).First().NOME;                    
                }
            }
                        
            return View(model);
        }

        [HttpPost]
        public ActionResult Preview(PreviewViewModel model)
        {
            try
            {
                int iNUMERO_PEDIDO_BLOCO = default(int);

                while (iNUMERO_PEDIDO_BLOCO == default(int))
                {
                    using (var ctx = new TIDalutexContext())
                    {
                        PROXIMO_NUMERO_PEDIDO reservar = ctx.PROXIMO_NUMERO_PEDIDO.Where(x => x.DISPONIVEL == 0).OrderBy(x => x.NUMERO_PEDIDO).First();
                        iNUMERO_PEDIDO_BLOCO = reservar.NUMERO_PEDIDO;
                        reservar.DISPONIVEL = 1;
                        ctx.SaveChanges();
                    }
                }

                #region Grava Pedido

                PRE_PEDIDO objPrePedido = new PRE_PEDIDO()
                {
                    NUMERO_PEDIDO_BLOCO = iNUMERO_PEDIDO_BLOCO,
                    TIPO_PEDIDO = base.Session_Carrinho.IDTipoPedido,
                    ID_REPRESENTANTE = base.Session_Carrinho.IDRepresentante,
                    ID_CLIENTE = base.Session_Carrinho.IDClienteFatura,
                    QUALIDADE_COM = base.Session_Carrinho.IDQualidadeComercial,
                    COD_COND_PGTO = base.Session_Carrinho.IDCondicoesPagto,
                    OBSERVACOES = base.Session_Carrinho.Observacoes,
                    DATA_ENTREGA = base.Session_Carrinho.DataEntrega,
                    DATA_EMISSAO = DateTime.Now,
                    DATA_EMISSAO_DT = DateTime.Today,
                    ID_CLIENTE_ENTREGA = base.Session_Carrinho.IDClienteEntrega,
                    ID_TRANSPORTADORA = base.Session_Carrinho.IDTransportadora,
                    USUARIO_INICIO = base.Session_Usuario.NOME_USU,
                    DATA_INICIO = DateTime.Now,
                    DATA_FINAL = DateTime.Now,
                    ID_LOCAL = base.Session_Carrinho.IDLocaisVenda,
                    COD_MOEDA = base.Session_Carrinho.IDMoedas,
                    CANAL_VENDAS = base.Session_Carrinho.IDCanaisVenda,
                    ATENDIMENTO = base.Session_Carrinho.IDTiposAtendimento,
                    TIPOFRETE = base.Session_Carrinho.IDFretes,
                    //GERENTE = model.IDGerentesVenda,// não é necessario gravar neste campo para pedidos <> de PE
                    VIATRANSPORTE = base.Session_Carrinho.IDViasTransporte,
                    COMISSAO = Session_Carrinho.PorcentagemComissao,
                    ORIGEM = "PW", // APENAS PRA INFORMAR QUE ESTE PEDIDO VEIO DO PEDIDO WEB NOVO. 
                    STATUS_PEDIDO = 1, //embora esteja definido no banco como padrão "1", esta gravando nulo, então, deixar explicito.....  
                    PEDIDO_CLIENTE = base.Session_Carrinho.PedidoCliente                                                                                                                                                                                                                                                                                               
                };
                #endregion

                using (var ctx = new TIDalutexContext())
                {
                    using (var transaction = ctx.Database.BeginTransaction())
                    {
                        try
                        {
                            List<PRE_PEDIDO_ITENS> lstItens = new List<PRE_PEDIDO_ITENS>();

                            int i = 0;
                            foreach (InserirNoCarrinhoViewModel item in base.Session_Carrinho.Itens)
                            {
                                if (base.Session_Carrinho.IDTipoPedido == (int)Enums.TiposPedido.RESERVA)
                                {
                                    item.Artigo = "0000";
                                    item.Quantidade = 100;
                                    item.Pecas = 1;
                                    item.TecnologiaPorExtenso = null;

                                    #region Reserva - Item sem reduzido (id_controle)

                                    if (item.Reduzido <= default(int))
                                    {
                                        item.Reduzido = ctx.Database.SqlQuery<int>("SELECT SEQ_ID_CONTROLE_DESENV.NEXTVAL FROM DUAL", 1).FirstOrDefault();

                                        CONTROLE_DESENV objInsert = new CONTROLE_DESENV()
                                        {
                                            ID_CONTROLE_DESENV = item.Reduzido,
                                            DT_ENT_ATEND = DateTime.Now,
                                            ID_USUARIO = base.Session_Usuario.COD_USU,
                                            ID_CLIENTE = base.Session_Carrinho.IDClienteFatura.ToString("000000"),
                                            ID_REP = base.Session_Carrinho.IDRepresentante,
                                            ID_STUDIO = item.IDStudio,
                                            ID_ITEM_STUDIO = item.IDItemStudio,
                                            STATUS_GERAL = 1
                                        };

                                        ctx.CONTROLE_DESENV.Add(objInsert);
                                    }

                                    CONTROLE_DESENV_ITEM_STUDIO objUpdate = ctx.CONTROLE_DESENV_ITEM_STUDIO.Find(item.IDItemStudio);
                                    objUpdate.STATUS = 1; //INDISPONÍVEL

                                    #endregion
                                }

                                i++;

                                string origem = "";

                                if (item.Tipo == Enums.ItemType.ValidacaoReserva)
                                {
                                    origem = "E";

                                    int _id = ctx.Database.SqlQuery<int>("SELECT SEQ_ID_PED_RES_VENDA.NEXTVAL FROM DUAL", 1).FirstOrDefault();

                                    ctx.PED_RESERVA_VENDA.Add(new PED_RESERVA_VENDA()
                                    {
                                        PEDIDO_RESERVA = item.PedidoReserva,
                                        ITEM_PED_RESERVA = item.ItemPedidoReserva,
                                        ID_VAR_PED_RESERVA = item.IDVariante,
                                        PEDIDO_VENDA = iNUMERO_PEDIDO_BLOCO,
                                        ITEM_PED_VENDA = i
                                        ,
                                        ID_PED_RESERVA_VENDA = _id
                                    });

                                    ctx.SaveChanges();
                                }

                                PRE_PEDIDO_ITENS objItem = new PRE_PEDIDO_ITENS()
                                {
                                    ARTIGO = item.Artigo,
                                    COR = item.Cor,
                                    DATA_ENTREGA = item.DataEntregaItem,
                                    DESENHO = item.Desenho,
                                    ITEM = i,
                                    LISO_ESTAMP = item.Tecnologia,
                                    NUMERO_PEDIDO_BLOCO = iNUMERO_PEDIDO_BLOCO,
                                    PE = "N",
                                    PRECO_UNIT = item.Preco,
                                    PRECOLISTA = item.PrecoTabela,
                                    QTDEPC = item.Pecas,
                                    QUANTIDADE = item.Quantidade,
                                    REDUZIDO_ITEM = item.Reduzido,
                                    UM = item.UnidadeMedida,
                                    VALOR_TOTAL = item.Preco * item.Quantidade,
                                    VARIANTE = item.Variante,
                                    COD_COMPOSE = item.Compose,
                                    ORIGEM = origem,
                                };

                                if (item.Tipo == Enums.ItemType.ValidacaoReserva || item.Tipo == Enums.ItemType.Estampado)
                                {
                                    if (item.TecnologiaOriginal != item.TecnologiaPorExtenso)
                                    {
                                        objItem.TROCA_TECNOLOGIA = "Troca de " + item.TecnologiaOriginal + " para " + item.TecnologiaPorExtenso;
                                    }
                                }

                                lstItens.Add(objItem);
                            }

                            ctx.PRE_PEDIDO.Add(objPrePedido);

                            if (base.Session_Carrinho.IDTipoPedido != (int)Enums.TiposPedido.RESERVA)
                            {
                                #region Criticas

                                #region Liberação financeira

                                ctx.PRE_PEDIDO_CRITICA.Add(new PRE_PEDIDO_CRITICA()
                                {
                                    NUMERO_PRE_PEDIDO = objPrePedido.NUMERO_PEDIDO_BLOCO,
                                    COD_CRITICA = (decimal)Enums.TiposCritica.LiberacaoFinanceira,
                                    FLG_STATUS = "C"
                                });

                                #endregion

                                #region Item sem reduzido

                                bool hasItemSemReduzido = false;

                                foreach (PRE_PEDIDO_ITENS item in lstItens)
                                {
                                    if (!hasItemSemReduzido && item.REDUZIDO_ITEM.GetValueOrDefault() == -2)
                                    {
                                        hasItemSemReduzido = true;
                                    }
                                }

                                if (hasItemSemReduzido)
                                {
                                    ctx.PRE_PEDIDO_CRITICA.Add(new PRE_PEDIDO_CRITICA()
                                    {
                                        NUMERO_PRE_PEDIDO = objPrePedido.NUMERO_PEDIDO_BLOCO,
                                        COD_CRITICA = (decimal)Enums.TiposCritica.SemReduzido,
                                        FLG_STATUS = "C"
                                    });
                                }

                                #endregion

                                #region Preço divergente

                                foreach (PRE_PEDIDO_ITENS item in lstItens)
                                {
                                    if (item.REDUZIDO_ITEM > 0)
                                    {
                                        //Se não tem preço, crítica. Se tem preço e ele é diferente do informado pelo representante, crítica.
                                        if (item.PRECOLISTA == null || item.PRECOLISTA.GetValueOrDefault() != item.PRECO_UNIT.GetValueOrDefault())
                                        {
                                            ctx.PRE_PEDIDO_CRITICA.Add(new PRE_PEDIDO_CRITICA()
                                            {
                                                NUMERO_PRE_PEDIDO = objPrePedido.NUMERO_PEDIDO_BLOCO,
                                                COD_CRITICA = (decimal)Enums.TiposCritica.PrecoDiferente,
                                                FLG_STATUS = "C",
                                                ITEM_PEDIDO = item.ITEM,
                                                VALOR_TAB = item.PRECOLISTA,
                                                VALOR_ITEM = item.PRECO_UNIT
                                            });
                                        }
                                    }
                                }

                                #endregion

                                #endregion
                            }

                            foreach (PRE_PEDIDO_ITENS item in lstItens)
                            {
                                ctx.PRE_PEDIDO_ITENS.Add(item);
                            }

                            ctx.SaveChanges();

                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw ex;
                        }
                    }
                }

                #region EnviarPDF

                //MemoryStream ms = new MemoryStream(new Relatorios().GerarEspelhoPedido());
                //Attachment anexo = new Attachment(ms, "Pedido_" + iNUMERO_PEDIDO_BLOCO.ToString() + ".pdf", "application/pdf");
                //Utilitarios util = new Utilitarios();
                //util.EnviaEmail("crmaimone@gmail.com", "Novo pedido", "Segue novo pedido", anexo);

                #endregion

                base.Session_Carrinho = null;

                return RedirectToAction("ConfirmacaoPedido", "Pedido", new { numeropedido = iNUMERO_PEDIDO_BLOCO.ToString() });
            }
            catch (Exception ex)
            {
                base.Handle(ex);
            }
            
            return View(model.Sucesso = false);
        }

        #endregion

        #region Coleções

        public ActionResult MenuColecoes(string idcolecao, string nmcolecao, string pagina, string totalpaginas)
        {
            MenuColecoesViewModel model = new MenuColecoesViewModel();
            model.Filtro = nmcolecao;

            if (!string.IsNullOrWhiteSpace(totalpaginas))
            {
                model.TotalPaginas = int.Parse(totalpaginas);
            }

            if (string.IsNullOrWhiteSpace(pagina))
                model.Pagina = 1;
            else
                model.Pagina = int.Parse(pagina);

            ActionResult objValidatorResult = this.ValidarTipoPedido(model);
            if(objValidatorResult != null)
            {
                return objValidatorResult;
            }
            
            int iIDColecao;

            if (int.TryParse(idcolecao, out iIDColecao))
            {
                if(iIDColecao > 0)
                {
                    using (var ctx = new TIDalutexContext())
                    {
                        VW_COLECAO objColecao = ctx.VW_COLECAO.Where(x => x.ID_COLECAO == iIDColecao).First();
                        model.Colecoes = new List<VW_COLECAO>();
                        model.Colecoes.Add(objColecao);
                        model.Filtro = objColecao.NOME_COLECAO;
                        model.Pagina = 1;
                    }
                }
            }
            else if (!string.IsNullOrWhiteSpace(model.Filtro))
            {
                ObterColecoes(model);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MenuColecoes(MenuColecoesViewModel model)
        {
            model.Pagina = 1;
            ObterColecoes(model);

            return View(model);
        }

        private void ObterColecoes(MenuColecoesViewModel model)
        {
            int iItensPorPagina = 10;

            using (var ctx = new TIDalutexContext())
            {
                List<VW_COLECAO> result = null;

                if(model.TotalPaginas == 0)
                {
                    result = ctx.VW_COLECAO
                                    .Where(x => x.NOME_COLECAO.ToUpper().Contains(model.Filtro.ToUpper()))
                                    .OrderBy(x => x.NOME_COLECAO)
                                    .ToList();
                    decimal dTotal = result.Count / (decimal)iItensPorPagina;
                    model.TotalPaginas = (int) Decimal.Ceiling(dTotal);
                    if (model.TotalPaginas == 0)
                        model.TotalPaginas = 1;

                    model.Colecoes = result.Skip((model.Pagina - 1) * iItensPorPagina).Take(iItensPorPagina).ToList();
                }
                else
                {
                    model.Colecoes = ctx.VW_COLECAO
                                    .Where(x => x.NOME_COLECAO.ToUpper().Contains(model.Filtro.ToUpper()))
                                    .OrderBy(x => x.NOME_COLECAO)
                                    .Skip((model.Pagina - 1) * iItensPorPagina)
                                    .Take(iItensPorPagina).ToList();
                }
            }
        }

        public ActionResult Desenhos(string idcolecao, string nmcolecao, string filtro, string pagina, string totalpaginas)
        {
            DesenhosViewModel model = new DesenhosViewModel();
            model.NMColecao = nmcolecao;
            model.FiltroDesenho = filtro;

            if (!string.IsNullOrWhiteSpace(totalpaginas))
            {
                model.TotalPaginas = int.Parse(totalpaginas);
            }

            if (string.IsNullOrWhiteSpace(pagina))
                model.Pagina = 1;
            else
                model.Pagina = int.Parse(pagina);

            ActionResult objValidatorResult = this.ValidarTipoPedido(model);
            if (objValidatorResult != null)
            {
                return objValidatorResult;
            }

            using (var ctx = new TIDalutexContext())
            {
                if (idcolecao == "ATUAL")
                {
                    CONFIG_GERAL objResult = ctx.CONFIG_GERAL.Find((int)Enums.TipoColecaoEspecial.Atual);
                    model.IDColecao = int.Parse(objResult.PARAMETRO1);
                    model.NMColecao = objResult.PARAMETRO2;
                }
                else if (idcolecao == "POCKET")
                {
                    CONFIG_GERAL objResult = ctx.CONFIG_GERAL.Find((int)Enums.TipoColecaoEspecial.Pocket);
                    model.IDColecao = int.Parse(objResult.INT1.ToString());
                    model.NMColecao = objResult.PARAMETRO2;
                }
                else if (idcolecao == "DESENHOS")
                {
                    model.IDColecao = -1;
                    model.NMColecao = "DESENHOS";
                }
                else if (idcolecao == null)
                {
                    ModelState.AddModelError("", "Coleção não informada.");
                    return View(model);
                }
                else
                {
                    model.IDColecao = int.Parse(idcolecao);
                }
            }

            if (model.TotalPaginas > 0 || model.IDColecao > 0)
            {
                ObterDesenhos(model);
            }

            model.UrlImagens = ConfigurationManager.AppSettings["PASTA_DESENHOS"];
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Desenhos(DesenhosViewModel model)
        {
            model.Pagina = 1;
            this.ObterDesenhos(model);
            model.UrlImagens = ConfigurationManager.AppSettings["PASTA_DESENHOS"];

            return View(model);
        }

        private void ObterDesenhos(DesenhosViewModel model)
        {
            int iItensPorPagina = 24;

            using (var ctx = new TIDalutexContext())
            {
                List<DesenhoVariante> result = null;
                
                if (model.IDColecao == -1) // se partiu da view de desenhos ...... (-1 é valor padrão para quando a coloção não é identificada).
                {                
                    var query =
                        from dc in ctx.VW_DESENHOS_POR_COLECAO
                        where
                            ((model.IDColecao == -1) || (dc.COLECAO == model.IDColecao))
                            && (dc.DESENHO.StartsWith(model.FiltroDesenho.ToUpper()) || model.FiltroDesenho == null)
                            && (dc.TECNOLOGIA.StartsWith(model.FiltroTecnologia.ToUpper()) || model.FiltroTecnologia == null)
                            && (dc.ARTIGO.StartsWith(model.FiltroArtigo.ToUpper()) || model.FiltroArtigo == null)
                        group dc by
                            new
                            {
                                dc.DESENHO,
                                dc.VARIANTE
                            }
                            into dv
                            select new DesenhoVariante
                            {
                                Desenho = dv.Key.DESENHO,
                                Variante = dv.Key.VARIANTE
                            };
                    if (model.TotalPaginas == 0)
                    {
                        result = query.OrderBy(x => x.Desenho)
                                            .ThenBy(x => x.Variante)
                                            .ToList();

                        decimal dTotal = result.Count / (decimal)iItensPorPagina;
                        model.TotalPaginas = (int)Decimal.Ceiling(dTotal);
                        if (model.TotalPaginas == 0)
                            model.TotalPaginas = 1;

                        model.Galeria = result.Skip((model.Pagina - 1) * iItensPorPagina).Take(iItensPorPagina).ToList();
                    }
                    else
                    {
                        model.Galeria = query.OrderBy(x => x.Desenho)
                                            .ThenBy(x => x.Variante)
                                            .Skip((model.Pagina - 1) * iItensPorPagina)
                                            .Take(iItensPorPagina)
                                            .ToList();
                    }
                }
                else//-- oda -- 03/09/2015 -- se for origem da view de coleções, então pega coleção do controle do desenvolvimento;
                {
                    var query =
                        from dc in ctx.VW_DESENHOS_POR_COL_DESENV
                        where
                            ((model.IDColecao == -1) || (dc.COLECAO == model.IDColecao))
                            && (dc.DESENHO.StartsWith(model.FiltroDesenho.ToUpper()) || model.FiltroDesenho == null)
                            && (dc.TECNOLOGIA.StartsWith(model.FiltroTecnologia.ToUpper()) || model.FiltroTecnologia == null)
                            && (dc.ARTIGO.StartsWith(model.FiltroArtigo.ToUpper()) || model.FiltroArtigo == null)
                        group dc by
                            new
                            {
                                dc.DESENHO,
                                dc.VARIANTE
                            }
                            into dv
                            select new DesenhoVariante
                            {
                                Desenho = dv.Key.DESENHO,
                                Variante = dv.Key.VARIANTE
                            };
                    if (model.TotalPaginas == 0)
                    {
                        result = query.OrderBy(x => x.Desenho)
                                            .ThenBy(x => x.Variante)
                                            .ToList();

                        decimal dTotal = result.Count / (decimal)iItensPorPagina;
                        model.TotalPaginas = (int)Decimal.Ceiling(dTotal);
                        if (model.TotalPaginas == 0)
                            model.TotalPaginas = 1;

                        model.Galeria = result.Skip((model.Pagina - 1) * iItensPorPagina).Take(iItensPorPagina).ToList();
                    }
                    else
                    {
                        model.Galeria = query.OrderBy(x => x.Desenho)
                                            .ThenBy(x => x.Variante)
                                            .Skip((model.Pagina - 1) * iItensPorPagina)
                                            .Take(iItensPorPagina)
                                            .ToList();
                    }
                }                
            }
        }

        public ActionResult Lisos(string idcolecao, string nmcolecao, string pagina, string totalpaginas)
        {
            LisosViewModel model = new LisosViewModel();
            model.NMColecao = nmcolecao;

            if (!string.IsNullOrWhiteSpace(totalpaginas))
            {
                model.TotalPaginas = int.Parse(totalpaginas);
            }

            if (string.IsNullOrWhiteSpace(pagina))
                model.Pagina = 1;
            else
                model.Pagina = int.Parse(pagina);

            ActionResult objValidatorResult = this.ValidarTipoPedido(model);
            if (objValidatorResult != null)
            {
                return objValidatorResult;
            }

            using (var ctx = new TIDalutexContext())
            {
                if (idcolecao == "ATUAL")
                {
                    CONFIG_GERAL objResult = ctx.CONFIG_GERAL.Find((int)Enums.TipoColecaoEspecial.Atual);
                    model.IDColecao = int.Parse(objResult.PARAMETRO1);
                    model.NMColecao = objResult.PARAMETRO2;
                }
                else if (idcolecao == "POCKET")
                {
                    CONFIG_GERAL objResult = ctx.CONFIG_GERAL.Find((int)Enums.TipoColecaoEspecial.Pocket);
                    model.IDColecao = int.Parse(objResult.INT1.ToString());
                    model.NMColecao = objResult.PARAMETRO2;
                }
                else if (idcolecao == "LISOS")
                {
                    model.IDColecao = -1;
                    model.NMColecao = "LISOS";
                }
                else if (idcolecao == null)
                {
                    ModelState.AddModelError("", "Coleção não informada.");
                    return View(model);
                }
                else
                {
                    model.IDColecao = int.Parse(idcolecao);
                }

                Utilitarios utils = new Utilitarios();

                int iItensPorPagina = 24;

                List<Liso> result = null;
                var query =
                    from dc in ctx.VW_LISOS_POR_COLECAO
                    where
                        (dc.COLECAO == model.IDColecao || model.IDColecao == -1)
                    select new Liso
                    {
                        Reduzido = dc.CODIGO_REDUZIDO,
                        Artigo = dc.ARTIGO,
                        Cor = dc.COR,
                        RGB = dc.CAMINHO
                    };


                if (model.TotalPaginas == 0)
                {
                    result = query.OrderBy(x => x.Cor).ThenBy(x => x.Artigo).ToList();

                    decimal dTotal = result.Count / (decimal)iItensPorPagina;
                    model.TotalPaginas = (int)Decimal.Ceiling(dTotal);
                    if (model.TotalPaginas == 0)
                        model.TotalPaginas = 1;

                    model.Galeria = result.Skip((model.Pagina - 1) * iItensPorPagina).Take(iItensPorPagina).ToList();
                }
                else
                {
                    model.Galeria = query.OrderBy(x => x.Cor)
                                            .ThenBy(x => x.Artigo)
                                            .Skip((model.Pagina - 1) * iItensPorPagina)
                                            .Take(iItensPorPagina)
                                            .ToList();
                }
                
                model.Galeria.ForEach(delegate(Liso item)
                {
                    item.RGB = utils.RGBConverter(ColorTranslator.FromWin32(int.Parse(item.RGB)));
                });
            }

            return View(model);
        }

        private void CarregarTiposPedidos(InserirNoCarrinhoViewModel model)
        {
            using (DalutexContext ctxDalutex = new DalutexContext())
            {
                int[] tiposPedidos = new int[] 
                { 
                    (int)Enums.TiposPedido.AMOSTRA,
                    (int)Enums.TiposPedido.PILOTAGEM,
                    (int)Enums.TiposPedido.VENDA,
                    (int)Enums.TiposPedido.MOSTRUARIO,
                };

                //model.TiposPedido = new SelectList(ctxDalutex.COML_TIPOSPEDIDOS.Where(x => tiposPedidos.Any(tipo => x.TIPOPEDIDO.Equals(tipo))).ToList().Select(x => new SelectListItem() { Text = x.DESCRICAO, Value = x.TIPOPEDIDO.ToString() }));
                model.TiposPedido = ctxDalutex.COML_TIPOSPEDIDOS.Where(x => tiposPedidos.Any(tipo => x.TIPOPEDIDO.Equals(tipo))).ToList();
            }
        }

        #endregion

        #region Pedido Reserva

        public ActionResult ItensParaReserva(string filtrocodstudio, string filtrocoddal, string filtrodesenho, string pagina, string totalpaginas)
        {
            ItensParaReservaViewModel model = new ItensParaReservaViewModel();
            model.FiltroCodStudio = filtrocodstudio;
            model.FiltroCodDal = filtrocoddal;
            model.FiltroDesenho = filtrodesenho;

            if (!string.IsNullOrWhiteSpace(totalpaginas))
            {
                model.TotalPaginas = int.Parse(totalpaginas);
            }

            if (string.IsNullOrWhiteSpace(pagina))
                model.Pagina = 1;
            else
                model.Pagina = int.Parse(pagina);

            ActionResult objValidatorResult = this.ValidarTipoPedido(model);
            if (objValidatorResult != null)
            {
                return objValidatorResult;
            }

            if(Session_Carrinho == null)
            {
                Session_Carrinho = new ConclusaoPedidoViewModel();
                Session_Carrinho.IDTipoPedido = (int)Enums.TiposPedido.RESERVA;
            }
            else
            {
                Session_Carrinho.IDTipoPedido = (int)Enums.TiposPedido.RESERVA;
            }

            ObterItensParaReserva(model);

            model.UrlImagens = ConfigurationManager.AppSettings["PASTA_RESERVAS"];
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ItensParaReserva(ItensParaReservaViewModel model)
        {
            model.Pagina = 1;
            this.ObterItensParaReserva(model);
            model.UrlImagens = ConfigurationManager.AppSettings["PASTA_RESERVAS"];

            return View(model);
        }

        private void ObterItensParaReserva(ItensParaReservaViewModel model)
        {
            int iItensPorPagina = 24;

            using (var ctx = new TIDalutexContext())
            {
                List<ItemReserva> result = null;

                var query =
                        from dc in ctx.VW_DESENHOS_DISP_RESERVA
                        where
                            (dc.DESENHO.StartsWith(model.FiltroDesenho.ToUpper()) || model.FiltroDesenho == null)
                            && (dc.COD_STUDIO.ToUpper().StartsWith(model.FiltroCodStudio.ToUpper()) || model.FiltroCodStudio == null)
                            && (dc.COD_DAL.ToUpper().Contains(model.FiltroCodDal.ToUpper()) || model.FiltroCodDal == null)
                        group dc by
                            new
                            {
                                dc.DESENHO,
                                dc.COD_STUDIO,
                                dc.COD_DAL,
                                dc.ID_CONTROLE_DESENV,
                                dc.ID_STUDIO,
                                dc.ID_ITEM_STUDIO
                            }
                            into dv
                            select new ItemReserva
                            {
                                Desenho = dv.Key.DESENHO.ToUpper(),
                                CodStudio = dv.Key.COD_STUDIO.ToUpper(),
                                CodDal = dv.Key.COD_DAL.ToUpper(),
                                IDControleDesenvolvimento = dv.Key.ID_CONTROLE_DESENV,
                                IDStudio = (int)dv.Key.ID_STUDIO,
                                IDItemStudio = (int)dv.Key.ID_ITEM_STUDIO
                            };


                if (model.TotalPaginas == 0)
                {
                    result = query.OrderByDescending(x => x.CodDal).ToList();

                    decimal dTotal = result.Count / (decimal)iItensPorPagina;
                    model.TotalPaginas = (int)Decimal.Ceiling(dTotal);
                    if (model.TotalPaginas == 0)
                        model.TotalPaginas = 1;

                    model.Galeria = result.Skip((model.Pagina - 1) * iItensPorPagina).Take(iItensPorPagina).ToList();
                }
                else
                {
                    model.Galeria = query.OrderByDescending(x => x.CodDal)
                                            .Skip((model.Pagina - 1) * iItensPorPagina)
                                            .Take(iItensPorPagina)
                                            .ToList();
                }
            }
        }

        public ActionResult DesenhosValidaReserva(int pedidoreserva, string pagina, string totalpaginas)
        {
            DesenhosValidaReservaViewModel model = new DesenhosValidaReservaViewModel();

            if (!string.IsNullOrWhiteSpace(totalpaginas))
            {
                model.TotalPaginas = int.Parse(totalpaginas);
            }

            if (string.IsNullOrWhiteSpace(pagina))
                model.Pagina = 1;
            else
                model.Pagina = int.Parse(pagina);

            model.UrlImagens = ConfigurationManager.AppSettings["PASTA_DESENHOS"];

            int iItensPorPagina = 24;

            List<VW_ITENS_VALIDAR_RESERVA> result = null;

            using (TIDalutexContext ctx = new TIDalutexContext())
            {
                if (model.TotalPaginas == 0)
                {
                    result = ctx.VW_ITENS_VALIDAR_RESERVA
                                        .Where(x => x.PEDIDO_RESERVA == pedidoreserva)
                                        .OrderBy(x => x.DESENHO)
                                        .ThenBy(x => x.VARIANTE)
                                        .ToList();

                    decimal dTotal = result.Count / (decimal)iItensPorPagina;
                    model.TotalPaginas = (int)Decimal.Ceiling(dTotal);
                    if (model.TotalPaginas == 0)
                        model.TotalPaginas = 1;

                    model.Galeria = result.Skip((model.Pagina - 1) * iItensPorPagina).Take(iItensPorPagina).ToList();
                }
                else
                {
                    model.Galeria = ctx.VW_ITENS_VALIDAR_RESERVA
                                        .Where(x => x.PEDIDO_RESERVA == pedidoreserva)
                                        .OrderBy(x => x.DESENHO)
                                        .ThenBy(x => x.VARIANTE)
                                        .Skip((model.Pagina - 1) * iItensPorPagina)
                                        .Take(iItensPorPagina)
                                        .ToList();
                }
            }

            return View(model);
        }

        public ActionResult ValidaPedidoReserva(
            string filtropedidoreserva
            , string filtrocliente
            , string filtrorepresentante
            , string filtrocodstudio
            , string filtrocoddal
            , string filtrodesenho
            , string pagina
            , string totalpaginas)
        {
            ValidaPedidoReservaViewModel model = new ValidaPedidoReservaViewModel();
            model.FiltroPedidoReserva = filtropedidoreserva;
            model.FiltroCliente = filtrocliente;
            model.FiltroRepresentante = filtrorepresentante;
            model.FiltroCodStudio = filtrocodstudio;
            model.FiltroCodDal = filtrocoddal;
            model.FiltroDesenho = filtrodesenho;

            ActionResult objValidatorResult = this.ValidarTipoPedido(model);
            if (objValidatorResult != null)
            {
                return objValidatorResult;
            }

            if (!string.IsNullOrWhiteSpace(totalpaginas))
            {
                model.TotalPaginas = int.Parse(totalpaginas);
            }

            if (string.IsNullOrWhiteSpace(pagina))
            {
                model.Pagina = 1;

                if (base.Session_Usuario.ID_REPRES > default(int))
                {
                    int IDRepresentante = base.Session_Usuario.ID_REPRES.GetValueOrDefault();
                    using (var ctx = new DalutexContext())
                    {
                        model.FiltroRepresentante = ctx.REPRESENTANTES.Find(IDRepresentante).NOME.Trim();
                    }

                    this.ObterItensValidacaoReserva(model);
                }
            }
            else
            {
                model.Pagina = int.Parse(pagina);

                this.ObterItensValidacaoReserva(model);
            }

            
            return View(model);
        }

        [HttpPost]
        public ActionResult ValidaPedidoReserva(ValidaPedidoReservaViewModel model)
        {
            model.Pagina = 1;
            this.ObterItensValidacaoReserva(model);

            return View(model);
        }

        private void ObterItensValidacaoReserva(ValidaPedidoReservaViewModel model)
        {
            int PedidoReserva = 0;
            int.TryParse(model.FiltroPedidoReserva, out PedidoReserva);
            int iItensPorPagina = 6;

            using (var ctx = new TIDalutexContext())
            {
                List<VW_VALIDAR_RESERVA> result = null;

                if (model.TotalPaginas == 0)
                {
                    result = ctx.VW_VALIDAR_RESERVA
                                .Where(
                                    x =>
                                    (model.FiltroRepresentante == null || x.REPRESENTANTE.StartsWith(model.FiltroRepresentante.ToUpper()))
                                    &&
                                    (model.FiltroCliente == null || x.CLIENTE.StartsWith(model.FiltroCliente.ToUpper()))
                                    &&
                                    (model.FiltroCodDal == null || x.COD_DAL.Contains(model.FiltroCodDal.ToUpper()))
                                    &&
                                    (model.FiltroDesenho == null || x.DESENHO.StartsWith(model.FiltroDesenho.ToUpper()))
                                    &&
                                    (model.FiltroCodStudio == null || x.COD_STUDIO.StartsWith(model.FiltroCodStudio.ToUpper()))
                                    &&
                                    (PedidoReserva == 0 || x.PEDIDO.Equals(PedidoReserva))
                                ).OrderByDescending(x => x.DATA_EMISSAO)
                                .ToList();

                    decimal dTotal = result.Count / (decimal)iItensPorPagina;
                    model.TotalPaginas = (int)Decimal.Ceiling(dTotal);
                    if (model.TotalPaginas == 0)
                        model.TotalPaginas = 1;

                    model.ListaValidaReserva = result.Skip((model.Pagina - 1) * iItensPorPagina).Take(iItensPorPagina).ToList();

                    if ((PedidoReserva != 0) && (result.Count() == 0))
                    {
                        ModelState.AddModelError("", "Pedido informado não tem VARIANTES (coloração) cadastradas no Desenvolvimento");
                    }
                }
                else
                {
                    model.ListaValidaReserva = ctx.VW_VALIDAR_RESERVA
                                .Where(
                                    x =>
                                    (model.FiltroRepresentante == null || x.REPRESENTANTE.StartsWith(model.FiltroRepresentante.ToUpper()))
                                    &&
                                    (model.FiltroCliente == null || x.CLIENTE.StartsWith(model.FiltroCliente.ToUpper()))
                                    &&
                                    (model.FiltroCodDal == null || x.COD_DAL.Contains(model.FiltroCodDal.ToUpper()))
                                    &&
                                    (model.FiltroDesenho == null || x.DESENHO.StartsWith(model.FiltroDesenho.ToUpper()))
                                    &&
                                    (model.FiltroCodStudio == null || x.COD_STUDIO.StartsWith(model.FiltroCodStudio.ToUpper()))
                                    &&
                                    (PedidoReserva == 0 || x.PEDIDO.Equals(PedidoReserva))
                                ).OrderByDescending(x => x.DATA_EMISSAO)
                                .Skip((model.Pagina - 1) * iItensPorPagina)
                                .Take(iItensPorPagina)
                                .ToList();

                    if ((PedidoReserva != 0) && (result.Count() == 0))
                    {
                        ModelState.AddModelError("", "Pedido informado não tem VARIANTES (coloração) cadastrada no Desenvolvimento.");
                    }
                }
            }
        }

        #endregion

        #region Pronta Entrega

        public ActionResult ItensProntaEntrega( string pagina, string totalpaginas)
        {
            ItensProntaEntregaViewModel model = new ItensProntaEntregaViewModel();
            model.Tipo = Enums.ItemType.ProntaEntrega;

            if (!string.IsNullOrWhiteSpace(totalpaginas))
            {
                model.TotalPaginas = int.Parse(totalpaginas);
            }

            if (string.IsNullOrWhiteSpace(pagina))
                model.Pagina = 1;
            else
                model.Pagina = int.Parse(pagina);
            
            model.UrlImagens = ConfigurationManager.AppSettings["PASTA_DESENHOS"];
	
            this.ObterDesenhosProntaEntrega(model);

            return View(model);
        }

        private void ObterDesenhosProntaEntrega(ItensProntaEntregaViewModel model)
        {            
            int iItensPorPagina = 24;

            using (var ctx = new TIDalutexContext())
            {
                List<VW_ITENS_PE> result = null;

                if (model.TotalPaginas == 0)
                {
                    result = ctx.VW_ITENS_PE.OrderByDescending(x => x.DESENHO)
                                .ThenBy(x => x.VARIANTE)
                                .ToList();

                    decimal dTotal = result.Count / (decimal)iItensPorPagina;
                    model.TotalPaginas = (int)Decimal.Ceiling(dTotal);
                    if (model.TotalPaginas == 0)
                        model.TotalPaginas = 1;

                    model.Galeria = result.Skip((model.Pagina - 1) * iItensPorPagina).Take(iItensPorPagina).ToList();
                }
                else
                {
                    model.Galeria = ctx.VW_ITENS_PE.OrderByDescending(x => x.DESENHO)
                                .ThenBy(x => x.VARIANTE)
                                .Skip((model.Pagina - 1) * iItensPorPagina)
                                .Take(iItensPorPagina)
                                .ToList();
                }
            }
        }

        public ActionResult DetalhesPE(int reduzido)
        {
            DetalhesPEViewModel model = new DetalhesPEViewModel();

            using(TIDalutexContext ctx = new TIDalutexContext())
            {
                model.ListaPecasPE = ctx.VW_LISTA_PECAS_PE.Where(x => x.REDUZIDO == reduzido).OrderBy(x => x.QUALIDADE).ToList();

                model.DetalhesReduzido = ctx.VW_ITENS_PE.Where(x => x.REDUZIDO == reduzido).ToList();
            }

            model.UrlImagens = model.UrlImagens = ConfigurationManager.AppSettings["PASTA_DESENHOS"]; ;
            model.Desenho = model.DetalhesReduzido.First(item => item.REDUZIDO == reduzido).DESENHO;
            model.Variante = model.DetalhesReduzido.First(item => item.REDUZIDO == reduzido).VARIANTE;
           
            return View(model);
        }

        #endregion
    }
}