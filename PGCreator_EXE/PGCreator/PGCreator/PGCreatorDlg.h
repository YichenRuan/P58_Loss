
// PGCreatorDlg.h : 头文件
//
#pragma once
#include "afxcmn.h"
#include "InfoDlg.h"
#include "LevelDlg.h"
#include "DefaultDlg.h"
#include "CompDlg.h"
#include "MateDlg.h"

#define MAX_SETTING_LENGTH 600

// CPGCreatorDlg 对话框
class CPGCreatorDlg : public CDialogEx
{
// 构造
public:
	CPGCreatorDlg(CWnd* pParent = NULL);	// 标准构造函数

// 对话框数据
	enum { IDD = IDD_PGCREATOR_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV 支持


// 实现
protected:
	HICON m_hIcon;

	// 生成的消息映射函数
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()
public:
	CInfoDlg m_infoDlg;
	CLevelDlg m_levelDlg;
	CDefaultDlg m_defaultDlg;
	CCompDlg m_compDlg;
	CMateDlg m_mateDlg;
	
	CTabCtrl m_tab;

	CBrush  m_brush;
	char inFileName[30];
	char outFileName[30];
	char settingFileName[30];
	char inFile[500];
	char inPath[MAX_PATH];
	char outPath[MAX_PATH];
	char settingPath[MAX_PATH];

	afx_msg void OnSelchangeTab(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg HBRUSH OnCtlColor(CDC* pDC, CWnd* pWnd, UINT nCtlColor);
	void ReadInFile();
	void UseInFile();
	afx_msg void OnBnClickedOk();
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	//char* GetProgramDir();
	afx_msg void OnBnClickedCancel();
	void static TcharToChar (const TCHAR * tchar, char * _char);
	void static CharToTchar (const char * _char, TCHAR * tchar);
};