#pragma once
#include "afxwin.h"


// CDefaultDlg 对话框

class CDefaultDlg : public CDialog
{
	DECLARE_DYNAMIC(CDefaultDlg)

public:
	CDefaultDlg(CWnd* pParent = NULL);   // 标准构造函数
	virtual ~CDefaultDlg();

// 对话框数据
	enum { IDD = IDD_DEFAULT_DIALOG};

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV 支持

	DECLARE_MESSAGE_MAP()
public:
	virtual BOOL PreTranslateMessage(MSG* pMsg);
//	CButton m_Rmf;
//	CButton m_Rsdc;
	virtual BOOL OnInitDialog();
	void OutputInfo(FILE* fp);
};
