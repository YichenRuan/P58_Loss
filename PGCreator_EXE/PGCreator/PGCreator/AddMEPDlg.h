#pragma once
#include <list>
using namespace std;

// CAddMEPDlg 对话框

class CAddMEPDlg : public CDialog
{
	DECLARE_DYNAMIC(CAddMEPDlg)

public:
	CAddMEPDlg(CWnd* pParent = NULL);   // 标准构造函数
	virtual ~CAddMEPDlg();

// 对话框数据
	enum { IDD = IDD_ADDMEP_DIALOG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV 支持

public:
	list<CString> MEPComp;
	list<CString> cate;
	list<list<CString>> type;

	DECLARE_MESSAGE_MAP()
public:
	int index_MEP, index_cate;
	list<int> sele_type;
	virtual BOOL OnInitDialog();
	void Synchronize(list<CString>& MEPComp0, list<CString>& cate0, list<list<CString>>& type0);
	afx_msg void OnBnClickedOk();
	afx_msg void OnSelchangeComboCatecode();
};
