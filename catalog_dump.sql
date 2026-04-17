--
-- PostgreSQL database dump
--

\restrict GMrSnE6RqnNviQGh0POAtS6lDQt2pXeNuau84F8BF9hgCUACveqBEVw5GkKYTmC

-- Dumped from database version 16.13 (Debian 16.13-1.pgdg13+1)
-- Dumped by pg_dump version 16.13 (Debian 16.13-1.pgdg13+1)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

ALTER TABLE ONLY public."__EFMigrationsHistory" DROP CONSTRAINT "PK___EFMigrationsHistory";
ALTER TABLE ONLY public."Products" DROP CONSTRAINT "PK_Products";
ALTER TABLE ONLY public."ProductComments" DROP CONSTRAINT "PK_ProductComments";
DROP TABLE public."__EFMigrationsHistory";
DROP TABLE public."Products";
DROP TABLE public."ProductComments";
SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: ProductComments; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."ProductComments" (
    "Id" uuid NOT NULL,
    "ProductId" uuid NOT NULL,
    "UserName" text NOT NULL,
    "Content" text NOT NULL,
    "CreatedDate" timestamp with time zone NOT NULL
);


--
-- Name: Products; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Products" (
    "Id" uuid NOT NULL,
    "Name" text NOT NULL,
    "Description" text NOT NULL,
    "Price" numeric NOT NULL,
    "StockQuantity" integer NOT NULL,
    "CreatedDate" timestamp with time zone NOT NULL,
    "Category" text DEFAULT ''::text NOT NULL,
    "ImageUrl" text DEFAULT ''::text NOT NULL,
    "Features" text[] DEFAULT ARRAY[]::text[] NOT NULL,
    "Rating" double precision DEFAULT 0.0 NOT NULL,
    "ReviewCount" integer DEFAULT 0 NOT NULL,
    "ImageUrls" text[] DEFAULT ARRAY[]::text[] NOT NULL
);


--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


--
-- Data for Name: ProductComments; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."ProductComments" ("Id", "ProductId", "UserName", "Content", "CreatedDate") FROM stdin;
1ef983c2-b40d-4b0c-a272-e14c37aa52ef	4c443b0a-113d-4b82-8604-aad665ec99b3	ahmetkoc	müthiş damnnn	2026-03-25 12:43:33.682553+00
b085bd3c-537b-4f18-aba6-53d2329f8d6e	d06bf12d-1e8e-43ab-a5f7-ef227fae92a4	ahmetkoc	müthiş	2026-03-26 09:52:58.750019+00
\.


--
-- Data for Name: Products; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."Products" ("Id", "Name", "Description", "Price", "StockQuantity", "CreatedDate", "Category", "ImageUrl", "Features", "Rating", "ReviewCount", "ImageUrls") FROM stdin;
bb00b766-2dd8-491d-b527-9776aa28fdc9	Monitor		1000	4	2026-03-25 10:04:29.238863+00	Monitör	/images/25f05eac-a1a7-4554-bae8-fbd04ddadeb3.jpg	{"144HZ Monitör"}	0	0	{}
bb1dc241-c41a-4eae-85a3-b260b219f59c	Kasa		4000	8	2026-03-25 10:04:08.525875+00	Kasa	/images/66a0bda8-7835-42a4-b15c-fe951f81c7b4.jpg	{"Oyuncu Kasası"}	0	0	{}
3a23dda3-0fb3-4b64-9d8a-928333aa7df4	Kulaklık		2500	19	2026-03-25 10:03:14.447896+00	Kulaklık	/images/84e996f6-e936-4ece-999d-20383897cb76.jpg	{"Kulaküstü Kulaklık","Oyuncu Kulaklığı"}	0	0	{}
d06bf12d-1e8e-43ab-a5f7-ef227fae92a4	Mouse		2000	19	2026-03-25 10:02:20.487581+00	Mouse	/images/8f3f3ff1-ef93-4269-b7b8-601923bc20d2.jpg	{"Logitech Mouse"}	4	1	{/images/a2b75c8e-3eaa-4f15-8a09-a40eb9c06039.jpeg}
4c443b0a-113d-4b82-8604-aad665ec99b3	Mekanik Klavye		3000	28	2026-03-25 10:01:53.867459+00	Klavye	/images/4d9eb791-d69a-43d5-ac89-3bec040267ec.jpg	{"RGB Aydınlatmalı"}	4	1	{}
\.


--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
20260316093537_InitialCreate	10.0.5
20260323083224_first	10.0.5
20260323125937_RenameStockToStockQuantity	10.0.5
20260324063605_AddImageUrlToProduct	10.0.5
20260324070049_AddProductMetaData	10.0.5
20260324071341_AddProductComments	10.0.5
20260408072654_AddImageUrls	10.0.5
\.


--
-- Name: ProductComments PK_ProductComments; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ProductComments"
    ADD CONSTRAINT "PK_ProductComments" PRIMARY KEY ("Id");


--
-- Name: Products PK_Products; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Products"
    ADD CONSTRAINT "PK_Products" PRIMARY KEY ("Id");


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- PostgreSQL database dump complete
--

\unrestrict GMrSnE6RqnNviQGh0POAtS6lDQt2pXeNuau84F8BF9hgCUACveqBEVw5GkKYTmC

