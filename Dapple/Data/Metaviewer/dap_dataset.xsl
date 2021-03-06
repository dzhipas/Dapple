<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="html" omit-xml-declaration="yes"/>
	
	<xsl:template match="/">
	<html>
		<head>
			<script type="text/javascript" src="geometatree.js"/>
			<link href="geosoft.meta.css" type="text/css" rel="stylesheet"/>
		</head>
		<body>
			<h3>
				<xsl:for-each select="/">
					<xsl:for-each select="dapmeta">
						<xsl:for-each select="name">
							<xsl:apply-templates/>
						</xsl:for-each>
					</xsl:for-each>
				</xsl:for-each>
			Metadata</h3>
			<div>
				<xsl:for-each select="/">
					<xsl:for-each select="dapmeta">
						<xsl:for-each select="meta">
							<xsl:apply-templates/>
						</xsl:for-each>
					</xsl:for-each>
				</xsl:for-each>
			</div>
		</body>
	</html>
	</xsl:template>

	<xsl:template match="Name">
	</xsl:template>
	<xsl:template match="CLASS">
		<span class="trigger">
			<xsl:attribute name="onClick">showBranch('<xsl:number count="*[@name]" level="any"/>');</xsl:attribute>
			<img src="expanded.gif">
				<xsl:attribute name="id">I<xsl:number count="*[@name]" level="any"/></xsl:attribute>
			</img>
			<xsl:text> </xsl:text>
			<xsl:value-of select="@name"/>
			<br/>
		</span>
		<span class="branch" style="display: block;">
			<xsl:attribute name="id"><xsl:number count="*[@name]" level="any"/></xsl:attribute>
			<xsl:apply-templates/>
		</span>
	</xsl:template>
	<xsl:template match="ATTRIBUTE">
		<xsl:value-of select="@name"/> : <xsl:value-of select="@value"/>
		<br/>
	</xsl:template>
	<xsl:template match="TABLE">
		<span class="trigger">
			<xsl:attribute name="onClick">showBranch('t<xsl:number count="*[@name]" level="any"/>');</xsl:attribute>
			<img src="collapsed.gif">
				<xsl:attribute name="id">It<xsl:number count="*[@name]" level="any"/></xsl:attribute>
			</img>
         Table
         <br/>
		</span>
		<span class="branch">
			<xsl:attribute name="id">t<xsl:number count="*[@name]" level="any"/></xsl:attribute>
			<xsl:apply-templates/>
		</span>
	</xsl:template>
	<xsl:template match="ITEM">
		<span class="trigger">
			<xsl:attribute name="onClick">showBranch('<xsl:number count="*[@name]" level="any"/>');</xsl:attribute>
			<img src="collapsed.gif">
				<xsl:attribute name="id">I<xsl:number count="*[@name]" level="any"/></xsl:attribute>
			</img>
			<xsl:value-of select="@name"/>
			<br/>
		</span>
		<span class="branch">
			<xsl:attribute name="id"><xsl:number count="*[@name]" level="any"/></xsl:attribute>
			<xsl:apply-templates/>
		</span>
	</xsl:template>
</xsl:stylesheet>
