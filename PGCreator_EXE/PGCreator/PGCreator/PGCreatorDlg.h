
// PGCreatorDlg.h : ͷ�ļ�
//
#pragma once
#include "afxcmn.h"
#include "InfoDlg.h"
#include "LevelDlg.h"
#include "DefaultDlg.h"
#include "CompDlg.h"
#include "MateDlg.h"

#define MAX_SETTING_LENGTH 600

// CPGCreatorDlg �Ի���
class CPGCreatorDlg : public CDialogEx
{
// ����
public:
	CPGCreatorDlg(CWnd* pParent = NULL);	// ��׼���캯��

// �Ի�������
	enum { IDD = IDD_PGCREATOR_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV ֧��


// ʵ��
protected:
	HICON m_hIcon;

	// ���ɵ���Ϣӳ�亯��
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