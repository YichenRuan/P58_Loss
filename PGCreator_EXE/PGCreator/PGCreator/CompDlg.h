#pragma once
//#include "MyListCtrl.h"
#define NUM_COMP 2
/*
	0: 梁柱节点
	1: 剪力墙
*/

// CCompDlg 对话框

class CCompDlg : public CDialog
{
	DECLARE_DYNAMIC(CCompDlg)

public:
	CCompDlg(CWnd* pParent = NULL);   // 标准构造函数
	virtual ~CCompDlg();

// 对话框数据
	enum { IDD = IDD_COMP_DIALOG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV 支持

	DECLARE_MESSAGE_MAP()
public:
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	virtual BOOL OnInitDialog();
	double price[NUM_COMP];
	int currItem,currSubItem;
	afx_msg void OnClickListComp(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnKillfocusEditPrice();
	double d_Eprice;
	afx_msg void OnItemchangedListComp(NMHDR *pNMHDR, LRESULT *pResult);
	void OutputInfo(FILE* fp);
};
