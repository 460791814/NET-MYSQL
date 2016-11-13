/*
Navicat MySQL Data Transfer

Source Server         : localhost_3306
Source Server Version : 50616
Source Host           : localhost:3306
Source Database       : jzhq

Target Server Type    : MYSQL
Target Server Version : 50616
File Encoding         : 65001

Date: 2014-08-29 17:29:13
*/

SET FOREIGN_KEY_CHECKS=0;
-- ----------------------------
-- Table structure for `t_keyword`
-- ----------------------------
DROP TABLE IF EXISTS `t_keyword`;
CREATE TABLE `t_keyword` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `KeyWord` varchar(100) DEFAULT NULL,
  `Hit` int(11) DEFAULT NULL,
  `UpdateTime` datetime DEFAULT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of t_keyword
-- ----------------------------
INSERT INTO `T_KeyWord` VALUES ('1', '1', '2', '2014-08-29 17:15:20');

-- ----------------------------
-- Table structure for `t_soft`
-- ----------------------------
DROP TABLE IF EXISTS `t_soft`;
CREATE TABLE `t_soft` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `Hash` varchar(50) NOT NULL,
  `Name` varchar(450) DEFAULT NULL,
  `Length` varchar(100) DEFAULT NULL,
  `Details` longtext,
  `SoftType` int(11) DEFAULT NULL,
  `FileCount` int(11) DEFAULT NULL,
  `Hit` int(11) DEFAULT NULL,
  `Area` int(11) DEFAULT NULL,
  `Publisher` varchar(100) DEFAULT NULL,
  `UpdateTime` datetime DEFAULT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of t_soft
-- ----------------------------
INSERT INTO `T_Soft` VALUES ('1', 'BC866536D537CBE42512077E2819801A916494E3', 'Man of Steel (2013)', '979.02 MB', '<?xml version=\"1.0\" encoding=\"utf-8\" ?><a><info><name><![CDATA[Man.of.Steel.2013.720p.BluRay.x264.YIFY.mp4]]></name><length>978.89 MB</length></info><info><name><![CDATA[WWW.YIFY-TORRENTS.COM.jpg]]></name><length>127.61 KB</length></info></a>', '2', '2', '1', '4', '', '2014-08-29 14:26:24');
INSERT INTO `T_Soft` VALUES ('2', 'A13FC4B7B48CAE597F15FE13D5981C5208860025', 'ONE MORE CHANCE (2007) [PINOY] DVDRiP DivX SoftEngSubs [Tagalog] WingTip', '813.21 MB', '<?xml version=\"1.0\" encoding=\"utf-8\" ?><a><info><name><![CDATA[IMPORTANT!! MUST READ THIS !!!.htm]]></name><length>2.59 KB</length></info><info><name><![CDATA[ONE MORE CHANCE (2007) [PINOY] DVDRiP DivX SoftEngSubs [Tagalog] WingTip.avi]]></name><length>809.92 MB</length></info><info><name><![CDATA[ONE MORE CHANCE (2007) [PINOY] DVDRiP DivX SoftEngSubs [Tagalog] WingTip.idx]]></name><length>61.69 KB</length></info><info><name><![CDATA[ONE MORE CHANCE (2007) [PINOY] DVDRiP DivX SoftEngSubs [Tagalog] WingTip.sub]]></name><length>3.22 MB</length></info></a>', '2', '4', '1', '4', '', '2014-08-29 14:26:54');
INSERT INTO `T_Soft` VALUES ('3', 'CEBBB18F8853B8E6A5F5E57991710F8BBD1653A5', 'Life of a King (2013) [1080p]', '1.64 GB', '<?xml version=\"1.0\" encoding=\"utf-8\" ?><a><info><name><![CDATA[Life.of.a.King.2013.1080p.BluRay.x264.YIFY.mp4]]></name><length>1.64 GB</length></info><info><name><![CDATA[WWW.YIFY-TORRENTS.COM.jpg]]></name><length>127.61 KB</length></info></a>', '2', '2', '1', '4', '', '2014-08-29 14:27:23');
INSERT INTO `T_Soft` VALUES ('4', 'B4B60566D00AD7769C3BDD6089FCA94521129CE0', '21 & Over (2013)', '751.61 MB', '<?xml version=\"1.0\" encoding=\"utf-8\" ?><a><info><name><![CDATA[21.&.Over.2013.720p.BluRay.x264.YIFY.mp4]]></name><length>751.37 MB</length></info><info><name><![CDATA[21.&.Over.2013.720p.BluRay.x264.YIFY.srt]]></name><length>110.99 KB</length></info><info><name><![CDATA[WWW.YIFY-TORRENTS.COM.jpg]]></name><length>127.61 KB</length></info></a>', '2', '3', '1', '4', '', '2014-08-29 15:29:49');
