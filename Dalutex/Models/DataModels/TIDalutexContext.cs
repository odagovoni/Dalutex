namespace Dalutex.Models.DataModels
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class TIDalutexContext : DbContext
    {
        public TIDalutexContext()
            : base("name=TIDalutexContext")
        {
        }

        public virtual DbSet<CONTROLE_DESENV> CONTROLE_DESENV { get; set; }
        public virtual DbSet<CONTROLE_DESENV_CARAC_TEC> CONTROLE_DESENV_CARAC_TEC { get; set; }
        public virtual DbSet<CONTROLE_DESENV_ITEM_STUDIO> CONTROLE_DESENV_ITEM_STUDIO { get; set; }
        public virtual DbSet<CONTROLE_DESENV_TECNOLOGIA> CONTROLE_DESENV_TECNOLOGIA { get; set; }
        public virtual DbSet<PED_ARTIGO_ATIVO> PED_ARTIGO_ATIVO { get; set; }
        public virtual DbSet<PED_ARTIGO_PROCESSO> PED_ARTIGO_PROCESSO { get; set; }
        public virtual DbSet<PED_TECNOLOGIA> PED_TECNOLOGIA { get; set; }
        public virtual DbSet<PRE_PEDIDO> PRE_PEDIDO { get; set; }
        public virtual DbSet<USUARIOS> USUARIOS { get; set; }
        public virtual DbSet<PRE_PEDIDO_ITENS> PRE_PEDIDO_ITENS { get; set; }
        public virtual DbSet<PED_LINK_RESTRICAO_ARTIGO> PED_LINK_RESTRICAO_ARTIGO { get; set; }
        public virtual DbSet<CONTROLE_DESENV_LINK_CT> CONTROLE_DESENV_LINK_CT { get; set; }
        public virtual DbSet<CONFIG_GERAL> CONFIG_GERAL { get; set; }
        public virtual DbSet<VW_CARACT_DESENHOS> VW_CARACT_DESENHOS { get; set; }
        public virtual DbSet<VW_COLECAO_ATUAL> VW_COLECAO_ATUAL { get; set; }
        public virtual DbSet<VW_ARTIGOS_DISPONIVEIS> VW_ARTIGOS_DISPONIVEIS { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VW_ARTIGOS_DISPONIVEIS>()
                .Property(e => e.ARTIGO)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<VW_ARTIGOS_DISPONIVEIS>()
                .Property(e => e.ID_TECNOLOGIA);

            modelBuilder.Entity<VW_ARTIGOS_DISPONIVEIS>()
                .Property(e => e.ID_CARAC_TEC);

            modelBuilder.Entity<VW_ARTIGOS_DISPONIVEIS>()
                .Property(e => e.TECNOLOGIA)
                .IsUnicode(false);

            modelBuilder.Entity<VW_CARACT_DESENHOS>()
                .Property(e => e.DESENHO)
                .IsUnicode(false);

            modelBuilder.Entity<VW_CARACT_DESENHOS>()
                .Property(e => e.ID_CARAC_TEC);

            modelBuilder.Entity<VW_CARACT_DESENHOS>()
                .Property(e => e.CARACT_TECNICA)
                .IsUnicode(false);

            modelBuilder.Entity<VW_CARACT_DESENHOS>()
                .Property(e => e.ID_TECNOLOGIA);

            modelBuilder.Entity<VW_CARACT_DESENHOS>()
                .Property(e => e.TECNOLOGIA)
                .IsUnicode(false);

            modelBuilder.Entity<VW_COLECAO_ATUAL>()
                .Property(e => e.DESENHO)
                .IsUnicode(false);

            modelBuilder.Entity<VW_COLECAO_ATUAL>()
                .Property(e => e.VARIANTE)
                .IsUnicode(false);

            modelBuilder.Entity<CONFIG_GERAL>()
                .Property(e => e.ID_CONFIG);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.ID_CONTROLE_DESENV)
                .HasPrecision(38, 0);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.ID_CLIENTE)
                .IsUnicode(false);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.CLIENTE_NOVO)
                .IsUnicode(false);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.ID_STUDIO)
                .HasPrecision(38, 0);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.COD_STUDIO)
                .IsUnicode(false);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.NOME_STUDIO)
                .IsUnicode(false);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.STATUS_CRIACAO)
                .HasPrecision(38, 0);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.OBSERVACAO_CRIACAO)
                .IsUnicode(false);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.ID_DESENHISTA_ART)
                .HasPrecision(38, 0);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.DESENHO)
                .IsUnicode(false);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.NOME_DESENHO)
                .IsUnicode(false);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.STATUS_ART)
                .HasPrecision(38, 0);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.OBSERVACAO_ART)
                .IsUnicode(false);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.ID_COLORISTA)
                .HasPrecision(38, 0);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.STATUS_ENVIO)
                .HasPrecision(38, 0);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.OBSERVACAO_ENVIO)
                .IsUnicode(false);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.STATUS_GERAL)
                .HasPrecision(38, 0);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.MOTIVO_CANCELAMENTO)
                .IsUnicode(false);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.USUARIO_CANCELAMENTO)
                .IsUnicode(false);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.TEM_CRIACAO)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.ID_ITEM_STUDIO)
                .HasPrecision(38, 0);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.ID_REP)
                .HasPrecision(38, 0);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.ID_TEMA)
                .HasPrecision(38, 0);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.ID_DESENHO1)
                .HasPrecision(38, 0);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.ID_DESENHO2)
                .HasPrecision(38, 0);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.ID_TECNOLOGIA)
                .HasPrecision(38, 0);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.FATURAR)
                .HasPrecision(38, 0);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.OBSERVACAO_ATEND)
                .IsUnicode(false);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.ID_USUARIO)
                .HasPrecision(38, 0);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.ORIGEM)
                .HasPrecision(38, 0);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.USUARIO_APROV_CANCEL)
                .IsUnicode(false);

            modelBuilder.Entity<CONTROLE_DESENV>()
                .Property(e => e.MOTIVO_APROV_CANCEL)
                .IsUnicode(false);

            modelBuilder.Entity<CONTROLE_DESENV_CARAC_TEC>()
                .Property(e => e.ID_CARAC_TEC)
                .HasPrecision(38, 0);

            modelBuilder.Entity<CONTROLE_DESENV_CARAC_TEC>()
                .Property(e => e.CARACT_TECNICA)
                .IsUnicode(false);

            modelBuilder.Entity<CONTROLE_DESENV_ITEM_STUDIO>()
                .Property(e => e.ID_ITEM_STUDIO)
                .HasPrecision(38, 0);

            modelBuilder.Entity<CONTROLE_DESENV_ITEM_STUDIO>()
                .Property(e => e.ID_STUDIO)
                .HasPrecision(38, 0);

            modelBuilder.Entity<CONTROLE_DESENV_ITEM_STUDIO>()
                .Property(e => e.COD_STUDIO)
                .IsUnicode(false);

            modelBuilder.Entity<CONTROLE_DESENV_ITEM_STUDIO>()
                .Property(e => e.COD_DAL)
                .IsUnicode(false);

            modelBuilder.Entity<CONTROLE_DESENV_ITEM_STUDIO>()
                .Property(e => e.ID_DESENHISTA)
                .HasPrecision(38, 0);

            modelBuilder.Entity<CONTROLE_DESENV_ITEM_STUDIO>()
                .Property(e => e.VALOR)
                .HasPrecision(38, 0);

            modelBuilder.Entity<CONTROLE_DESENV_ITEM_STUDIO>()
                .Property(e => e.MOEDA)
                .HasPrecision(38, 0);

            modelBuilder.Entity<CONTROLE_DESENV_ITEM_STUDIO>()
                .Property(e => e.STATUS)
                .HasPrecision(38, 0);

            modelBuilder.Entity<CONTROLE_DESENV_ITEM_STUDIO>()
                .Property(e => e.DESENHO)
                .IsUnicode(false);

            modelBuilder.Entity<CONTROLE_DESENV_ITEM_STUDIO>()
                .Property(e => e.OLD_DAL)
                .IsUnicode(false);

            modelBuilder.Entity<CONTROLE_DESENV_ITEM_STUDIO>()
                .Property(e => e.CLIENTE_COMPROU)
                .IsUnicode(false);

            modelBuilder.Entity<CONTROLE_DESENV_ITEM_STUDIO>()
                .Property(e => e.OBSERVACAO)
                .IsUnicode(false);

            modelBuilder.Entity<CONTROLE_DESENV_ITEM_STUDIO>()
                .Property(e => e.IMG_PGTO)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<CONTROLE_DESENV_TECNOLOGIA>()
                .Property(e => e.ID_TEC)
                .HasPrecision(38, 0);

            modelBuilder.Entity<CONTROLE_DESENV_TECNOLOGIA>()
                .Property(e => e.DESC_TEC)
                .IsUnicode(false);

            modelBuilder.Entity<PED_ARTIGO_ATIVO>()
                .Property(e => e.ARTIGO)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<PED_ARTIGO_PROCESSO>()
                .Property(e => e.PROCESSO)
                .IsUnicode(false);

            modelBuilder.Entity<PED_TECNOLOGIA>()
                .Property(e => e.ID_TECNOLOGIA)
                .HasPrecision(38, 0);

            modelBuilder.Entity<PED_TECNOLOGIA>()
                .Property(e => e.TECNOLOGIA)
                .IsUnicode(false);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.NUMERO_PRE_PEDIDO)
                .HasPrecision(38, 0);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.NUMERO_PEDIDO_BLOCO)
                .HasPrecision(38, 0);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.ID_REPRESENTANTE)
                .HasPrecision(38, 0);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.ID_CLIENTE)
                .HasPrecision(38, 0);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.STATUS_PEDIDO)
                .HasPrecision(38, 0);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.CLIENTE_NOVO)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.ESTADO)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.TIPO_PEDIDO)
                .HasPrecision(38, 0);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.USUARIO_STATUS)
                .IsUnicode(false);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.QUALIDADE_COM)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.COD_COND_PGTO)
                .HasPrecision(38, 0);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.OBSERVACOES)
                .IsUnicode(false);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.NOME_CLIENTE)
                .IsUnicode(false);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.ORIGEM)
                .IsUnicode(false);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.ID_CLIENTE_ENTREGA)
                .HasPrecision(38, 0);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.ID_TRANSPORTADORA)
                .HasPrecision(38, 0);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.USUARIO_INICIO)
                .IsUnicode(false);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.USUARIO_FINAL)
                .IsUnicode(false);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.CNPJ)
                .IsUnicode(false);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.FLAG_DATA_OK_APS)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.ID_LOCAL)
                .HasPrecision(38, 0);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.ID_STATUS)
                .HasPrecision(38, 0);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.CNPJ_ENTREGA)
                .IsUnicode(false);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.GERENTE)
                .HasPrecision(38, 0);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.ATENDIMENTO)
                .HasPrecision(38, 0);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.NUMERO_CARTAO)
                .HasPrecision(38, 0);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.BANDEIRA)
                .IsUnicode(false);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.BANCO)
                .IsUnicode(false);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.MOTIVO_CANC)
                .IsUnicode(false);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.COMISSAO)
                .HasPrecision(4, 2);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.ID_CLI_FACCAO)
                .HasPrecision(38, 0);

            modelBuilder.Entity<PRE_PEDIDO>()
                .Property(e => e.CNPJ_FACCAO)
                .IsUnicode(false);

            modelBuilder.Entity<USUARIOS>()
                .Property(e => e.COD_USU)
                .HasPrecision(38, 0);

            modelBuilder.Entity<USUARIOS>()
                .Property(e => e.NOME_USU)
                .IsUnicode(false);

            modelBuilder.Entity<USUARIOS>()
                .Property(e => e.LOGIN_USU)
                .IsUnicode(false);

            modelBuilder.Entity<USUARIOS>()
                .Property(e => e.SENHA_USU)
                .IsUnicode(false);

            modelBuilder.Entity<USUARIOS>()
                .Property(e => e.ADMINISTRADOR)
                .HasPrecision(38, 0);

            modelBuilder.Entity<USUARIOS>()
                .Property(e => e.TIPO_USUARIO)
                .HasPrecision(38, 0);

            modelBuilder.Entity<USUARIOS>()
                .Property(e => e.ID_REPRES)
                .HasPrecision(38, 0);

            modelBuilder.Entity<USUARIOS>()
                .Property(e => e.SETOR)
                .IsUnicode(false);

            modelBuilder.Entity<USUARIOS>()
                .Property(e => e.SID_SIMULT)
                .HasPrecision(38, 0);

            modelBuilder.Entity<PRE_PEDIDO_ITENS>()
                .Property(e => e.NUMERO_PRE_PEDIDO)
                .HasPrecision(38, 0);

            modelBuilder.Entity<PRE_PEDIDO_ITENS>()
                .Property(e => e.NUMERO_PEDIDO_BLOCO)
                .HasPrecision(38, 0);

            modelBuilder.Entity<PRE_PEDIDO_ITENS>()
                .Property(e => e.REDUZIDO_ITEM)
                .HasPrecision(38, 0);

            modelBuilder.Entity<PRE_PEDIDO_ITENS>()
                .Property(e => e.STATUS_ITEM)
                .HasPrecision(38, 0);

            modelBuilder.Entity<PRE_PEDIDO_ITENS>()
                .Property(e => e.LISO_ESTAMP)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<PRE_PEDIDO_ITENS>()
                .Property(e => e.MALHA_PLANO)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<PRE_PEDIDO_ITENS>()
                .Property(e => e.MODA_DECORACAO)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<PRE_PEDIDO_ITENS>()
                .Property(e => e.ARTIGO)
                .IsUnicode(false);

            modelBuilder.Entity<PRE_PEDIDO_ITENS>()
                .Property(e => e.COR)
                .IsUnicode(false);

            modelBuilder.Entity<PRE_PEDIDO_ITENS>()
                .Property(e => e.DESENHO)
                .IsUnicode(false);

            modelBuilder.Entity<PRE_PEDIDO_ITENS>()
                .Property(e => e.VARIANTE)
                .IsUnicode(false);

            modelBuilder.Entity<PRE_PEDIDO_ITENS>()
                .Property(e => e.QUANTIDADE)
                .HasPrecision(10, 2);

            modelBuilder.Entity<PRE_PEDIDO_ITENS>()
                .Property(e => e.PRECO_UNIT)
                .HasPrecision(10, 2);

            modelBuilder.Entity<PRE_PEDIDO_ITENS>()
                .Property(e => e.VALOR_TOTAL)
                .HasPrecision(10, 2);

            modelBuilder.Entity<PRE_PEDIDO_ITENS>()
                .Property(e => e.UM)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<PRE_PEDIDO_ITENS>()
                .Property(e => e.COLECAO)
                .HasPrecision(38, 0);

            modelBuilder.Entity<PRE_PEDIDO_ITENS>()
                .Property(e => e.PE)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<PRE_PEDIDO_ITENS>()
                .Property(e => e.ITEM)
                .HasPrecision(38, 0);

            modelBuilder.Entity<PRE_PEDIDO_ITENS>()
                .Property(e => e.SIT_ITEM)
                .HasPrecision(38, 0);

            modelBuilder.Entity<PRE_PEDIDO_ITENS>()
                .Property(e => e.ORIGEM)
                .IsUnicode(false);

            modelBuilder.Entity<PRE_PEDIDO_ITENS>()
                .Property(e => e.COMPOSE)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<PRE_PEDIDO_ITENS>()
                .Property(e => e.COD_COMPOSE)
                .HasPrecision(38, 0);

            modelBuilder.Entity<PRE_PEDIDO_ITENS>()
                .Property(e => e.ID_TAB_PRECO)
                .HasPrecision(38, 0);

            modelBuilder.Entity<PRE_PEDIDO_ITENS>()
                .Property(e => e.QUALIDADE)
                .HasPrecision(38, 0);

            modelBuilder.Entity<PRE_PEDIDO_ITENS>()
                .Property(e => e.PRECODIGITADOMOEDA)
                .HasPrecision(38, 0);

            modelBuilder.Entity<PRE_PEDIDO_ITENS>()
                .Property(e => e.PRECOLISTA)
                .HasPrecision(38, 0);

            modelBuilder.Entity<PRE_PEDIDO_ITENS>()
                .Property(e => e.QTDEPC)
                .HasPrecision(38, 0);
        }
    }
}