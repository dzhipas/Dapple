<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:fn="http://www.w3.org/2005/xpath-functions" xmlns:xdt="http://www.w3.org/2005/xpath-datatypes" xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:wms="http://www.opengis.net/wms">
	<xsl:output version="1.0" method="html" indent="no" encoding="UTF-8"/>
	<xsl:template match="/">
		<html>
			<head>
				<script type="text/javascript" src="geometatree.js"/>
				<link href="geosoft.meta.css" type="text/css" rel="stylesheet"/>
			</head>
			<body>
				<xsl:for-each select="WMT_MS_Capabilities | WMS_Capabilities | wms:WMS_Capabilities">
					<xsl:apply-templates select="Layer | wms:Layer">
						<xsl:with-param name="pstrIndent" select="''"/>
					</xsl:apply-templates>
				</xsl:for-each>
				<br/>
				<xsl:for-each select="/">
					<xsl:for-each select="WMT_MS_Capabilities | WMS_Capabilities | wms:WMS_Capabilities">
						<xsl:for-each select="Service | wms:Service">
							<xsl:for-each select="Title | wms:Title">
								<h2>
									Server Metadata: <xsl:apply-templates/>
								</h2>
							</xsl:for-each>
						</xsl:for-each>
					</xsl:for-each>
					<h3>
						<xsl:text>Abstract</xsl:text>
					</h3>
					<xsl:for-each select="WMT_MS_Capabilities | WMS_Capabilities | wms:WMS_Capabilities">
						<xsl:for-each select="Service | wms:Service">
							<xsl:for-each select="Abstract | wms:Abstract">
								<p>
									<xsl:apply-templates/>
								</p>
							</xsl:for-each>
						</xsl:for-each>
					</xsl:for-each>
					<br/>
					<h3>
						<xsl:text>Contact Information</xsl:text>
					</h3>
					<b>
						<xsl:text>Primary Contact: </xsl:text>
					</b>
					<xsl:for-each select="WMT_MS_Capabilities | WMS_Capabilities | wms:WMS_Capabilities">
						<xsl:for-each select="Service | wms:Service">
							<xsl:for-each select="ContactInformation | wms:ContactInformation">
								<xsl:for-each select="ContactPersonPrimary | wms:ContactPersonPrimary">
									<xsl:for-each select="ContactPerson | wms:ContactPerson">
										<xsl:apply-templates/>
									</xsl:for-each>
								</xsl:for-each>
							</xsl:for-each>
						</xsl:for-each>
					</xsl:for-each>
					<br/>
					<b>
						<xsl:text>Organization: </xsl:text>
					</b>
					<xsl:for-each select="WMT_MS_Capabilities | WMS_Capabilities | wms:WMS_Capabilities">
						<xsl:for-each select="Service | wms:Service">
							<xsl:for-each select="ContactInformation | wms:ContactInformation">
								<xsl:for-each select="ContactPersonPrimary | wms:ContactPersonPrimary">
									<xsl:for-each select="ContactOrganization | wms:ContactOrganization">
										<xsl:apply-templates/>
									</xsl:for-each>
								</xsl:for-each>
							</xsl:for-each>
						</xsl:for-each>
					</xsl:for-each>
					<br/>
					<b>
						<xsl:text>Position: </xsl:text>
					</b>
					<xsl:for-each select="WMT_MS_Capabilities | WMS_Capabilities | wms:WMS_Capabilities">
						<xsl:for-each select="Service | wms:Service">
							<xsl:for-each select="ContactInformation | wms:ContacctInformation">
								<xsl:for-each select="ContactPosition | wms:ContactPosition">
									<xsl:apply-templates/>
								</xsl:for-each>
							</xsl:for-each>
						</xsl:for-each>
					</xsl:for-each>
					<br/>
					<table border="0" cellpadding="0" cellspacing="0">
						<tr>
							<td>
								<b>
									<xsl:text>Address: </xsl:text>
								</b>
							</td>
							<td>
								<xsl:for-each select="WMT_MS_Capabilities | WMS_Capabilities | wms:WMS_Capabilities">
									<xsl:for-each select="Service | wms:Service">
										<xsl:for-each select="ContactInformation | wms:ContactInformation">
											<xsl:for-each select="ContactAddress | wms:ContactAddress">
												<xsl:for-each select="Address | wms:Address">
													<xsl:apply-templates/>
												</xsl:for-each>
											</xsl:for-each>
										</xsl:for-each>
									</xsl:for-each>
								</xsl:for-each>
							</td>
						</tr>
						<tr>
							<td></td>
							<td>
								<xsl:for-each select="WMT_MS_Capabilities | WMS_Capabilities | wms:WMS_Capabilities">
									<xsl:for-each select="Service | wms:Service">
										<xsl:for-each select="ContactInformation | wms:ContactInformation">
											<xsl:for-each select="ContactAddress | wms:ContactAddress">
												<xsl:for-each select="City | wms:City">
													<xsl:apply-templates/>
												</xsl:for-each>
											</xsl:for-each>
										</xsl:for-each>
									</xsl:for-each>
								</xsl:for-each>
							</td>
						</tr>
						<tr>
							<td></td>
							<td>
								<xsl:for-each select="WMT_MS_Capabilities | WMS_Capabilities | wms:WMS_Capabilities">
									<xsl:for-each select="Service | wms:Service">
										<xsl:for-each select="ContactInformation | wms:ContactInformation">
											<xsl:for-each select="ContactAddress | wms:ContactAddress">
												<xsl:for-each select="StateOrProvince | wms:StateOrProvince">
													<xsl:apply-templates/>
												</xsl:for-each>
											</xsl:for-each>
										</xsl:for-each>
									</xsl:for-each>
								</xsl:for-each>
							</td>
						</tr>
						<tr>
							<td></td>
							<td>
								<xsl:for-each select="WMT_MS_Capabilities | WMS_Capabilities | wms:WMS_Capabilities">
									<xsl:for-each select="Service | wms:Service">
										<xsl:for-each select="ContactInformation | wms:ContactInformation">
											<xsl:for-each select="ContactAddress | wms:ContactAddress">
												<xsl:for-each select="PostCode | wms:PostCode">
													<xsl:apply-templates/>
												</xsl:for-each>
											</xsl:for-each>
										</xsl:for-each>
									</xsl:for-each>
								</xsl:for-each>
							</td>
						</tr>
						<tr>
							<td></td>
							<td>
								<xsl:for-each select="WMT_MS_Capabilities | WMS_Capabilities | wms:WMS_Capabilities">
									<xsl:for-each select="Service | wms:Service">
										<xsl:for-each select="ContactInformation | wms:ContactInformation">
											<xsl:for-each select="ContactAddress | wms:ContactAddress">
												<xsl:for-each select="Country | wms:Country">
													<xsl:apply-templates/>
												</xsl:for-each>
											</xsl:for-each>
										</xsl:for-each>
									</xsl:for-each>
								</xsl:for-each>
							</td>
						</tr>
					</table>
					<b>
						<xsl:text>Voice Phone Number: </xsl:text>
					</b>
					<xsl:for-each select="WMT_MS_Capabilities | WMS_Capabilities | wms:WMS_Capabilities">
						<xsl:for-each select="Service | wms:Service">
							<xsl:for-each select="ContactInformation | wms:ContactInformation">
								<xsl:for-each select="ContactVoiceTelephone | wms:ContactVoiceTelephone">
									<xsl:apply-templates/>
								</xsl:for-each>
							</xsl:for-each>
						</xsl:for-each>
					</xsl:for-each>
					<br/>
					<b>
						<xsl:text>Fax Number: </xsl:text>
					</b>
					<xsl:for-each select="WMT_MS_Capabilities | WMS_Capabilities | wms:WMS_Capabilities">
						<xsl:for-each select="Service | wms:Service">
							<xsl:for-each select="ContactInformation | wms:ContactInformation">
								<xsl:for-each select="ContactFacsimileTelephone | wms:ContactFacsimilieTelephone">
									<xsl:apply-templates/>
								</xsl:for-each>
							</xsl:for-each>
						</xsl:for-each>
					</xsl:for-each>
					<br/>
					<b>
						<xsl:text>Email Address: </xsl:text>
					</b>
					<xsl:for-each select="WMT_MS_Capabilities | WMS_Capabilities | wms:WMS_Capabilities">
						<xsl:for-each select="Service | wms:Service">
							<xsl:for-each select="ContactInformation | wms:ContactInformation">
								<xsl:for-each select="ContactElectronicMailAddress | wms:ContactElectronicMailAddress">
									<a>
										<xsl:attribute name="href">
											mailto:<xsl:value-of select="."/>
										</xsl:attribute>
										<xsl:value-of select="."/>
									</a>
								</xsl:for-each>
							</xsl:for-each>
						</xsl:for-each>
					</xsl:for-each>
					<br/>
				</xsl:for-each>
			</body>
		</html>
	</xsl:template>
	<xsl:template match="Layer | wms:Layer">
		<xsl:param name="pstrIndent"/>
		<xsl:variable name="vthisIndent" select="concat($pstrIndent, '-')"/>
		<span>
			<h2>
				Layer Metadata: <xsl:value-of select="Title | wms:Title"/>
			</h2>
		</span>
		<span class="branch" style="display: block;">
			<xsl:attribute name="id">
				<xsl:number count="*[Title]" level="any"/>
			</xsl:attribute>
			<xsl:for-each select="Abstract | wms:Abstract">
				<br/>
				<b>Abstract:</b>
				<br/>
				<xsl:apply-templates/>
				<br/>
			</xsl:for-each>
			<xsl:for-each select="KeywordList | wms:KeywordList">
				<br/>
				<b>Keywords:</b>
				<br/>
				<xsl:for-each select="Keyword | wms:Keyword">
					<xsl:apply-templates/>,
				</xsl:for-each>
				<xsl:apply-templates/>
				<br/>
			</xsl:for-each>
			<xsl:for-each select="Style | wms:Style">
				<br/>
				<b>Legend: </b>
				<a>
					<xsl:attribute name="target">
						_blank
					</xsl:attribute>
					<xsl:attribute name="href">
						<xsl:for-each select="LegendURL | wms:LegendURL">
							<xsl:for-each select="OnlineResource | wms:OnlineResource">
								<xsl:value-of select="@xlink:href"/>
							</xsl:for-each>
						</xsl:for-each>
					</xsl:attribute>
					<xsl:for-each select="Title | wms:Title">
						<xsl:apply-templates/>
					</xsl:for-each>
				</a>
				<br/>
			</xsl:for-each>
			<xsl:for-each select="LatLonBoundingBox">
				<br/>
				<b>Lat/Lon Bounding Box:</b>
				<br/>
				minlon = <xsl:value-of select="@minx"/>
				, maxlon = <xsl:value-of select="@maxx"/><br/>
				minlat = <xsl:value-of select="@miny"/>
				, maxlat = <xsl:value-of select="@maxy"/><br/>
			</xsl:for-each>
			<xsl:for-each select="EX_GeographicBoundingBox">
				<br/>
				<b>Lat/Lon Bounding Box:</b>
				<br/>
				minlon = <xsl:value-of select="wms:westBoundLongitude"/>
				, maxlon = <xsl:value-of select="eastBoundLongitude"/><br/>
				minlat = <xsl:value-of select="southBoundLatitude"/>
				, maxlat = <xsl:value-of select="northBoundLatitude"/><br/>
			</xsl:for-each>
			<xsl:for-each select="wms:EX_GeographicBoundingBox">
				<br/>
				<b>Lat/Lon Bounding Box:</b>
				<br/>
				minlon = <xsl:value-of select="wms:westBoundLongitude"/>
				, maxlon = <xsl:value-of select="wms:eastBoundLongitude"/><br/>
				minlat = <xsl:value-of select="wms:southBoundLatitude"/>
				, maxlat = <xsl:value-of select="wms:northBoundLatitude"/><br/>
			</xsl:for-each>
			<br/>
			<b>Coordinate Systems</b>
			<br/>
			<xsl:for-each select="SRS | wms:SRS">
				<xsl:apply-templates/>
				<br/>
			</xsl:for-each>
			<xsl:for-each select="CRS | wms:CRS">
				<xsl:apply-templates/>
				<br/>
			</xsl:for-each>
			<br/>
		</span>
		<br/>
	</xsl:template>
</xsl:stylesheet>
